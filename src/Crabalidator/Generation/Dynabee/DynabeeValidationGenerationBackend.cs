using System.Reflection;
using Crabalidator.Configuration;
using Crabalidator.Planning;
using DynaBee;
using DynaBee.FluentApi;
using DynaBee.FluentApi.Body;
using DynaBee.FluentApi.DependencyInjection;
using DynaBee.FluentApi.Invocation;

namespace Crabalidator.Generation.Dynabee
{
    /// <summary>
    /// Compiles validation plans using DynaBee runtime type generation.
    /// </summary>
    internal sealed class DynabeeValidationGenerationBackend : IValidationGenerationBackend
    {
        private const string PlanPropertyName = "Plan";
        private const string ValidateMethodName = "Validate";
        private static readonly MethodInfo CreateTypedInvokerCoreMethod = typeof(DynabeeValidationGenerationBackend)
            .GetMethod(nameof(CreateTypedInvokerCore), BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo ValidationPlanExecuteMethod = typeof(ValidationPlan)
            .GetMethod(nameof(ValidationPlan.Execute), new[] { typeof(object) });
        private static readonly MethodInfo RuntimeShouldValidateMethod = typeof(DynabeeValidationRuntime)
            .GetMethod(nameof(DynabeeValidationRuntime.ShouldValidate));
        private static readonly MethodInfo RuntimeGetValueMethod = typeof(DynabeeValidationRuntime)
            .GetMethod(nameof(DynabeeValidationRuntime.GetValue));
        private static readonly MethodInfo RuntimeIsRuleInvalidMethod = typeof(DynabeeValidationRuntime)
            .GetMethod(nameof(DynabeeValidationRuntime.IsRuleInvalid));
        private static readonly MethodInfo RuntimeAddFailureMethod = typeof(DynabeeValidationRuntime)
            .GetMethod(nameof(DynabeeValidationRuntime.AddFailure));
        private static readonly MethodInfo RuntimeAddNestedFailuresMethod = typeof(DynabeeValidationRuntime)
            .GetMethod(nameof(DynabeeValidationRuntime.AddNestedFailures));
        private static readonly MethodInfo RuntimeToResultMethod = typeof(DynabeeValidationRuntime)
            .GetMethod(nameof(DynabeeValidationRuntime.ToResult));
        private static readonly MethodInfo StringIsNullOrWhiteSpaceMethod = typeof(string)
            .GetMethod(nameof(string.IsNullOrWhiteSpace), new[] { typeof(string) });

        private readonly IDynaBeeAssemblyBuilderFactory _builderFactory;
        private int _sequence;

        public DynabeeValidationGenerationBackend()
            : this(new DynaBeeAssemblyBuilderFactory())
        {
        }

        public DynabeeValidationGenerationBackend(IDynaBeeAssemblyBuilderFactory builderFactory)
        {
            _builderFactory = builderFactory ?? throw new ArgumentNullException(nameof(builderFactory));
        }

        public string Name => "Dynabee";

        public bool Supports(ValidationPlan plan)
            => plan != null && !plan.RequiresAsync;

        public CompiledValidator Compile(ValidationPlan plan)
        {
            if (plan == null)
            {
                throw new ArgumentNullException(nameof(plan));
            }

            if (!Supports(plan))
            {
                throw new NotSupportedException($"The {Name} backend only supports sync validation plans.");
            }

            var className = BuildClassName(plan);
            var context = _builderFactory
                .Create($"Crabalidator.Generated.{Interlocked.Increment(ref _sequence)}")
                .DisableCache()
                .AddClass(className, c => c
                    .RegisterAsConcrete(false)
                    .Inject<ValidationPlan>(PlanPropertyName)
                    .AddMethod(ValidateMethodName, typeof(ValidationResult), m => m
                        .WithParameter("instance", plan.ModelType)
                        .EmitsBody(body => EmitValidateBody(body, plan))))
                .Build();

            var validator = context.CreateInstance(className, plan);
            var invoker = context.CreateArgumentListAdapter(
                className,
                validator,
                ValidateMethodName,
                new[] { plan.ModelType });
            var typedInvoker = CreateTypedInvoker(context, className, validator, plan.ModelType);

            return new CompiledValidator(
                validator,
                validator.GetType(),
                new DynabeeCompiledValidatorInvoker(invoker),
                typedInvoker,
                plan);
        }

        private static object CreateTypedInvoker(
            IAssemblyContext context,
            string className,
            object validator,
            Type modelType)
            => CreateTypedInvokerCoreMethod
                .MakeGenericMethod(modelType)
                .Invoke(null, new[] { context, className, validator });

        private static object CreateTypedInvokerCore<T>(
            IAssemblyContext context,
            string className,
            object validator)
        {
            var validate = context.CreateBoundDelegate<Func<T, ValidationResult>>(
                className,
                validator,
                ValidateMethodName,
                new[] { typeof(T) });

            return new DynabeeTypedCompiledValidatorInvoker<T>(validate);
        }

        private static void EmitValidateBody(IBeeMethodBodyBuilder body, ValidationPlan plan)
        {
            var planExpression = body.Property(body.Self(), PlanPropertyName);
            var instance = body.Parameter("instance");
            var instanceAsObject = body.Convert(instance, typeof(object));

            if (!plan.ModelType.IsValueType)
            {
                body.If(
                    body.IsNull(instance),
                    branch => branch.Return(branch.Call(
                        branch.Property(branch.Self(), PlanPropertyName),
                        ValidationPlanExecuteMethod,
                        branch.Convert(branch.Parameter("instance"), typeof(object)))));
            }

            var failures = body.DeclareLocal<List<ValidationFailure>>("failures");
            body.Assign(failures, body.Constant(null, typeof(List<ValidationFailure>)));

            for (var propertyIndex = 0; propertyIndex < plan.Properties.Count; propertyIndex++)
            {
                EmitProperty(body, planExpression, instance, instanceAsObject, failures, plan.Properties[propertyIndex], propertyIndex);
            }

            body.Return(body.StaticCall(RuntimeToResultMethod, failures));
        }

        private static void EmitProperty(
            IBeeMethodBodyBuilder body,
            IBeeValueExpression planExpression,
            IBeeValueExpression instance,
            IBeeValueExpression instanceAsObject,
            IBeeLocal failures,
            PropertyValidationPlan property,
            int propertyIndex)
        {
            if (property.HasCondition)
            {
                body.If(
                    body.StaticCall(
                        RuntimeShouldValidateMethod,
                        planExpression,
                        body.Constant(propertyIndex),
                        instanceAsObject),
                    branch => EmitPropertyWithoutCondition(
                        branch,
                        branch.Property(branch.Self(), PlanPropertyName),
                        branch.Parameter("instance"),
                        branch.Convert(branch.Parameter("instance"), typeof(object)),
                        failures,
                        property,
                        propertyIndex));
                return;
            }

            EmitPropertyWithoutCondition(body, planExpression, instance, instanceAsObject, failures, property, propertyIndex);
        }

        private static void EmitPropertyWithoutCondition(
            IBeeMethodBodyBuilder body,
            IBeeValueExpression planExpression,
            IBeeValueExpression instance,
            IBeeValueExpression instanceAsObject,
            IBeeLocal failures,
            PropertyValidationPlan property,
            int propertyIndex)
        {
            var valueType = CanEmitDirectGetter(property, instance.Type) ? property.PropertyType : typeof(object);
            var attemptedValue = body.DeclareLocal($"value{propertyIndex}", valueType);

            if (CanEmitDirectGetter(property, instance.Type))
            {
                body.Assign(attemptedValue, body.Property(instance, property.PropertyName));
            }
            else
            {
                body.Assign(
                    attemptedValue,
                    body.StaticCall(RuntimeGetValueMethod, planExpression, body.Constant(propertyIndex), instanceAsObject));
            }

            IBeeLocal propertyFailed = null;
            if (property.CascadeMode == CascadeMode.Stop && property.Rules.Count > 1)
            {
                propertyFailed = body.DeclareLocal<bool>($"property{propertyIndex}Failed");
                body.Assign(propertyFailed, body.Constant(false));
            }

            for (var ruleIndex = 0; ruleIndex < property.Rules.Count; ruleIndex++)
            {
                var rule = property.Rules[ruleIndex];
                if (propertyFailed == null)
                {
                    EmitRule(body, planExpression, failures, attemptedValue, valueType, property, rule, propertyIndex, ruleIndex, propertyFailed);
                    continue;
                }

                body.If(
                    body.Not(propertyFailed),
                    branch => EmitRule(
                        branch,
                        branch.Property(branch.Self(), PlanPropertyName),
                        failures,
                        attemptedValue,
                        valueType,
                        property,
                        rule,
                        propertyIndex,
                        ruleIndex,
                        propertyFailed));
            }

            if (property.NestedValidation != null)
            {
                body.Assign(
                    failures,
                    body.StaticCall(
                        RuntimeAddNestedFailuresMethod,
                        failures,
                        planExpression,
                        body.Constant(propertyIndex),
                        body.Convert(attemptedValue, typeof(object)),
                        body.Constant(null, typeof(string))));
            }
        }

        private static void EmitRule(
            IBeeMethodBodyBuilder body,
            IBeeValueExpression planExpression,
            IBeeLocal failures,
            IBeeValueExpression attemptedValue,
            Type attemptedValueType,
            PropertyValidationPlan property,
            RulePlan rule,
            int propertyIndex,
            int ruleIndex,
            IBeeLocal propertyFailed)
        {
            var invalid = TryEmitInvalidExpression(body, attemptedValue, attemptedValueType, rule);
            invalid ??= body.StaticCall(
                RuntimeIsRuleInvalidMethod,
                planExpression,
                body.Constant(propertyIndex),
                body.Constant(ruleIndex),
                body.Convert(attemptedValue, typeof(object)));

            body.If(
                invalid,
                branch =>
                {
                    branch.Assign(
                        failures,
                        branch.StaticCall(
                            RuntimeAddFailureMethod,
                            failures,
                            branch.Property(branch.Self(), PlanPropertyName),
                            branch.Constant(propertyIndex),
                            branch.Constant(ruleIndex),
                            branch.Convert(attemptedValue, typeof(object)),
                            branch.Constant(null, typeof(string))));

                    if (propertyFailed != null)
                    {
                        branch.Assign(propertyFailed, branch.Constant(true));
                    }
                });
        }

        private static IBeeValueExpression TryEmitInvalidExpression(
            IBeeMethodBodyBuilder body,
            IBeeValueExpression value,
            Type valueType,
            RulePlan rule)
        {
            if (valueType == typeof(object))
            {
                return null;
            }

            return rule.Kind switch
            {
                RuleKind.NotNull => CanBeNull(valueType)
                    ? body.IsNull(value)
                    : body.Constant(false),
                RuleKind.NotEmpty => EmitNotEmptyInvalid(body, value, valueType),
                RuleKind.Equal => EmitEqualInvalid(body, value, valueType, rule.ComparisonValue),
                RuleKind.NotEqual => EmitNotEqualInvalid(body, value, valueType, rule.ComparisonValue),
                RuleKind.GreaterThan => EmitComparableInvalid(body, value, valueType, rule.ComparisonValue, RuleKind.GreaterThan),
                RuleKind.GreaterThanOrEqualTo => EmitComparableInvalid(body, value, valueType, rule.ComparisonValue, RuleKind.GreaterThanOrEqualTo),
                RuleKind.LessThan => EmitComparableInvalid(body, value, valueType, rule.ComparisonValue, RuleKind.LessThan),
                RuleKind.LessThanOrEqualTo => EmitComparableInvalid(body, value, valueType, rule.ComparisonValue, RuleKind.LessThanOrEqualTo),
                RuleKind.Length => EmitStringLengthInvalid(body, value, valueType, rule.Minimum, rule.Maximum),
                RuleKind.MinimumLength => EmitStringMinimumLengthInvalid(body, value, valueType, rule.Minimum),
                RuleKind.MaximumLength => EmitStringMaximumLengthInvalid(body, value, valueType, rule.Maximum),
                _ => null
            };
        }

        private static IBeeValueExpression EmitNotEmptyInvalid(IBeeMethodBodyBuilder body, IBeeValueExpression value, Type valueType)
        {
            if (valueType == typeof(string))
            {
                return body.StaticCall(StringIsNullOrWhiteSpaceMethod, value);
            }

            if (valueType.IsValueType && Nullable.GetUnderlyingType(valueType) == null)
            {
                return body.Equal(value, body.Default(valueType));
            }

            return null;
        }

        private static IBeeValueExpression EmitEqualInvalid(
            IBeeMethodBodyBuilder body,
            IBeeValueExpression value,
            Type valueType,
            object comparisonValue)
            => CanEmitDirectConstant(valueType, comparisonValue)
                ? body.NotEqual(value, body.Constant(comparisonValue, valueType))
                : null;

        private static IBeeValueExpression EmitNotEqualInvalid(
            IBeeMethodBodyBuilder body,
            IBeeValueExpression value,
            Type valueType,
            object comparisonValue)
            => CanEmitDirectConstant(valueType, comparisonValue)
                ? body.Equal(value, body.Constant(comparisonValue, valueType))
                : null;

        private static IBeeValueExpression EmitComparableInvalid(
            IBeeMethodBodyBuilder body,
            IBeeValueExpression value,
            Type valueType,
            object comparisonValue,
            RuleKind kind)
        {
            if (!CanEmitNumericConstant(valueType, comparisonValue))
            {
                return null;
            }

            var expected = body.Constant(comparisonValue, valueType);
            return kind switch
            {
                RuleKind.GreaterThan => body.LessThanOrEqual(value, expected),
                RuleKind.GreaterThanOrEqualTo => body.LessThan(value, expected),
                RuleKind.LessThan => body.GreaterThanOrEqual(value, expected),
                RuleKind.LessThanOrEqualTo => body.GreaterThan(value, expected),
                _ => null
            };
        }

        private static IBeeValueExpression EmitStringLengthInvalid(
            IBeeMethodBodyBuilder body,
            IBeeValueExpression value,
            Type valueType,
            int? minimum,
            int? maximum)
        {
            if (valueType != typeof(string) || minimum == null || maximum == null)
            {
                return null;
            }

            var length = body.Property(value, nameof(string.Length));
            return body.AndAlso(
                body.Not(body.IsNull(value)),
                body.OrElse(
                    body.LessThan(length, body.Constant(minimum.Value)),
                    body.GreaterThan(length, body.Constant(maximum.Value))));
        }

        private static IBeeValueExpression EmitStringMinimumLengthInvalid(
            IBeeMethodBodyBuilder body,
            IBeeValueExpression value,
            Type valueType,
            int? minimum)
        {
            if (valueType != typeof(string) || minimum == null)
            {
                return null;
            }

            return body.AndAlso(
                body.Not(body.IsNull(value)),
                body.LessThan(body.Property(value, nameof(string.Length)), body.Constant(minimum.Value)));
        }

        private static IBeeValueExpression EmitStringMaximumLengthInvalid(
            IBeeMethodBodyBuilder body,
            IBeeValueExpression value,
            Type valueType,
            int? maximum)
        {
            if (valueType != typeof(string) || maximum == null)
            {
                return null;
            }

            return body.AndAlso(
                body.Not(body.IsNull(value)),
                body.GreaterThan(body.Property(value, nameof(string.Length)), body.Constant(maximum.Value)));
        }

        private static bool CanEmitDirectGetter(PropertyValidationPlan property, Type modelType)
        {
            if (property.PropertyPath != property.PropertyName || !modelType.IsVisible)
            {
                return false;
            }

            var reflectedProperty = modelType.GetProperty(property.PropertyName, BindingFlags.Public | BindingFlags.Instance);
            return reflectedProperty?.GetMethod?.IsPublic == true;
        }

        private static bool CanEmitDirectConstant(Type valueType, object comparisonValue)
            => comparisonValue == null
                ? CanBeNull(valueType)
                : comparisonValue.GetType() == valueType && (valueType.IsPrimitive || valueType.IsEnum || valueType == typeof(string));

        private static bool CanEmitNumericConstant(Type valueType, object comparisonValue)
        {
            if (comparisonValue == null || comparisonValue.GetType() != valueType)
            {
                return false;
            }

            return valueType == typeof(byte)
                || valueType == typeof(short)
                || valueType == typeof(int)
                || valueType == typeof(long)
                || valueType == typeof(float)
                || valueType == typeof(double)
                || valueType == typeof(decimal);
        }

        private static bool CanBeNull(Type type)
            => !type.IsValueType || Nullable.GetUnderlyingType(type) != null;

        private static string BuildClassName(ValidationPlan plan)
            => $"{Sanitize(plan.ModelType.Name)}Validator";

        private static string Sanitize(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "Model";
            }

            var buffer = new char[name.Length];
            for (var i = 0; i < name.Length; i++)
            {
                var c = name[i];
                buffer[i] = char.IsLetterOrDigit(c) || c == '_' ? c : '_';
            }

            return new string(buffer);
        }
    }
}
