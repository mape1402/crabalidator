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
        private const string DelegatesPropertyName = "Delegates";
        private const string ValidateMethodName = "Validate";
        private static readonly MethodInfo CreateTypedInvokerCoreMethod = typeof(DynabeeValidationGenerationBackend)
            .GetMethod(nameof(CreateTypedInvokerCore), BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo ValidationPlanExecuteMethod = typeof(ValidationPlan)
            .GetMethod(nameof(ValidationPlan.Execute), new[] { typeof(object) });
        private static readonly MethodInfo RuntimeShouldValidateMethod = typeof(DynabeeValidationRuntime)
            .GetMethod(nameof(DynabeeValidationRuntime.ShouldValidate));
        private static readonly MethodInfo RuntimeGetValueMethod = typeof(DynabeeValidationRuntime)
            .GetMethod(nameof(DynabeeValidationRuntime.GetValue));
        private static readonly MethodInfo RuntimeGetNestedPlanMethod = typeof(DynabeeValidationRuntime)
            .GetMethod(nameof(DynabeeValidationRuntime.GetNestedPlan));
        private static readonly MethodInfo RuntimeIsRuleInvalidMethod = typeof(DynabeeValidationRuntime)
            .GetMethod(nameof(DynabeeValidationRuntime.IsRuleInvalid));
        private static readonly MethodInfo RuntimeAddNestedFailuresMethod = typeof(DynabeeValidationRuntime)
            .GetMethod(nameof(DynabeeValidationRuntime.AddNestedFailures));
        private static readonly MethodInfo RuntimeToResultMethod = typeof(DynabeeValidationRuntime)
            .GetMethod(nameof(DynabeeValidationRuntime.ToResult));
        private static readonly MethodInfo StringIsNullOrWhiteSpaceMethod = typeof(string)
            .GetMethod(nameof(string.IsNullOrWhiteSpace), new[] { typeof(string) });
        private static readonly MethodInfo FailureListAddMethod = typeof(List<ValidationFailure>)
            .GetMethod(nameof(List<ValidationFailure>.Add), new[] { typeof(ValidationFailure) });

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
            var delegates = BuildDelegateTable(plan);
            var context = _builderFactory
                .Create($"Crabalidator.Generated.{Interlocked.Increment(ref _sequence)}")
                .DisableCache()
                .AddClass(className, c => c
                    .RegisterAsConcrete(false)
                    .Inject<ValidationPlan>(PlanPropertyName)
                    .Inject<Delegate[]>(DelegatesPropertyName)
                    .AddMethod(ValidateMethodName, typeof(ValidationResult), m => m
                        .WithParameter("instance", plan.ModelType)
                        .EmitsBody(body => EmitValidateBody(body, plan, delegates))))
                .Build();

            var validator = context.CreateInstance(className, plan, delegates.Items);
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

        private static void EmitValidateBody(IBeeMethodBodyBuilder body, ValidationPlan plan, DelegateTable delegates)
        {
            var planExpression = body.Property(body.Self(), PlanPropertyName);
            var delegatesExpression = body.Property(body.Self(), DelegatesPropertyName);
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

            EmitProperties(body, planExpression, delegatesExpression, instance, instanceAsObject, failures, plan, delegates, null, null, "root");

            body.Return(body.StaticCall(RuntimeToResultMethod, failures));
        }

        private static void EmitProperties(
            IBeeMethodBodyBuilder body,
            IBeeValueExpression planExpression,
            IBeeValueExpression delegatesExpression,
            IBeeValueExpression instance,
            IBeeValueExpression instanceAsObject,
            IBeeLocal failures,
            ValidationPlan plan,
            DelegateTable delegates,
            string pathPrefix,
            IBeeValueExpression pathPrefixExpression,
            string localPrefix)
        {
            for (var propertyIndex = 0; propertyIndex < plan.Properties.Count; propertyIndex++)
            {
                EmitProperty(
                    body,
                    planExpression,
                    delegatesExpression,
                    instance,
                    instanceAsObject,
                    failures,
                    plan.Properties[propertyIndex],
                    propertyIndex,
                    plan.ModelType,
                    delegates,
                    pathPrefix,
                    pathPrefixExpression,
                    $"{localPrefix}_{propertyIndex}");
            }
        }

        private static void EmitProperty(
            IBeeMethodBodyBuilder body,
            IBeeValueExpression planExpression,
            IBeeValueExpression delegatesExpression,
            IBeeValueExpression instance,
            IBeeValueExpression instanceAsObject,
            IBeeLocal failures,
            PropertyValidationPlan property,
            int propertyIndex,
            Type modelType,
            DelegateTable delegates,
            string pathPrefix,
            IBeeValueExpression pathPrefixExpression,
            string localPrefix)
        {
            if (property.HasCondition)
            {
                var typedCondition = TryEmitTypedCondition(body, delegatesExpression, instance, property, modelType, delegates);
                if (typedCondition != null)
                {
                    body.If(
                        typedCondition,
                        branch => EmitPropertyWithoutCondition(
                            branch,
                            planExpression,
                            delegatesExpression,
                            instance,
                            instanceAsObject,
                            failures,
                            property,
                            propertyIndex,
                            modelType,
                            delegates,
                            pathPrefix,
                            pathPrefixExpression,
                            localPrefix));
                    return;
                }

                body.If(
                    body.StaticCall(
                        RuntimeShouldValidateMethod,
                        planExpression,
                        body.Constant(propertyIndex),
                        instanceAsObject),
                    branch => EmitPropertyWithoutCondition(
                        branch,
                        branch.Property(branch.Self(), PlanPropertyName),
                        branch.Property(branch.Self(), DelegatesPropertyName),
                        instance,
                        instanceAsObject,
                        failures,
                        property,
                        propertyIndex,
                        modelType,
                        delegates,
                        pathPrefix,
                        pathPrefixExpression,
                        localPrefix));
                return;
            }

            EmitPropertyWithoutCondition(body, planExpression, delegatesExpression, instance, instanceAsObject, failures, property, propertyIndex, modelType, delegates, pathPrefix, pathPrefixExpression, localPrefix);
        }

        private static void EmitPropertyWithoutCondition(
            IBeeMethodBodyBuilder body,
            IBeeValueExpression planExpression,
            IBeeValueExpression delegatesExpression,
            IBeeValueExpression instance,
            IBeeValueExpression instanceAsObject,
            IBeeLocal failures,
            PropertyValidationPlan property,
            int propertyIndex,
            Type modelType,
            DelegateTable delegates,
            string pathPrefix,
            IBeeValueExpression pathPrefixExpression,
            string localPrefix)
        {
            var valueType = CanEmitDirectGetter(property, instance.Type) ? property.PropertyType : typeof(object);
            var attemptedValue = body.DeclareLocal($"value_{localPrefix}", valueType);

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
                propertyFailed = body.DeclareLocal<bool>($"property_{localPrefix}_failed");
                body.Assign(propertyFailed, body.Constant(false));
            }

            for (var ruleIndex = 0; ruleIndex < property.Rules.Count; ruleIndex++)
            {
                var rule = property.Rules[ruleIndex];
                if (propertyFailed == null)
                {
                    EmitRule(body, planExpression, delegatesExpression, failures, attemptedValue, valueType, property, rule, propertyIndex, ruleIndex, propertyFailed, delegates, pathPrefix, pathPrefixExpression);
                    continue;
                }

                body.If(
                    body.Not(propertyFailed),
                    branch => EmitRule(
                        branch,
                        branch.Property(branch.Self(), PlanPropertyName),
                        branch.Property(branch.Self(), DelegatesPropertyName),
                        failures,
                        attemptedValue,
                        valueType,
                        property,
                        rule,
                        propertyIndex,
                        ruleIndex,
                        propertyFailed,
                        delegates,
                        pathPrefix,
                        pathPrefixExpression));
            }

            if (property.NestedValidation != null)
            {
                EmitNestedValidation(body, planExpression, delegatesExpression, failures, attemptedValue, property, propertyIndex, pathPrefix, pathPrefixExpression, localPrefix, delegates);
            }
        }

        private static void EmitRule(
            IBeeMethodBodyBuilder body,
            IBeeValueExpression planExpression,
            IBeeValueExpression delegatesExpression,
            IBeeLocal failures,
            IBeeValueExpression attemptedValue,
            Type attemptedValueType,
            PropertyValidationPlan property,
            RulePlan rule,
            int propertyIndex,
            int ruleIndex,
            IBeeLocal propertyFailed,
            DelegateTable delegates,
            string pathPrefix,
            IBeeValueExpression pathPrefixExpression)
        {
            var invalid = TryEmitInvalidExpression(body, delegatesExpression, attemptedValue, attemptedValueType, rule, delegates);
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
                    EmitAddFailure(branch, failures, attemptedValue, rule, pathPrefix, pathPrefixExpression);

                    if (propertyFailed != null)
                    {
                        branch.Assign(propertyFailed, branch.Constant(true));
                    }
                });
        }

        private static void EmitAddFailure(
            IBeeMethodBodyBuilder body,
            IBeeLocal failures,
            IBeeValueExpression attemptedValue,
            RulePlan rule,
            string pathPrefix,
            IBeeValueExpression pathPrefixExpression)
        {
            body.If(
                body.IsNull(failures),
                branch => branch.Assign(failures, branch.New<List<ValidationFailure>>()));

            var propertyPath = pathPrefixExpression == null
                ? body.Constant(CombinePath(pathPrefix, rule.Failure.PropertyPath), typeof(string))
                : AppendPath(body, pathPrefixExpression, rule.Failure.PropertyPath);

            body.Evaluate(body.Call(
                failures,
                FailureListAddMethod,
                body.New(
                    typeof(ValidationFailure),
                    body.Constant(rule.Failure.PropertyName, typeof(string)),
                    propertyPath,
                    body.Constant(rule.Failure.ErrorMessage, typeof(string)),
                    body.Convert(attemptedValue, typeof(object)),
                    body.Constant(rule.Failure.ErrorCode, typeof(string)),
                    body.Convert(body.Constant((int)rule.Failure.Severity), typeof(ValidationSeverity)))));
        }

        private static void EmitNestedValidation(
            IBeeMethodBodyBuilder body,
            IBeeValueExpression planExpression,
            IBeeValueExpression delegatesExpression,
            IBeeLocal failures,
            IBeeValueExpression attemptedValue,
            PropertyValidationPlan property,
            int propertyIndex,
            string pathPrefix,
            IBeeValueExpression pathPrefixExpression,
            string localPrefix,
            DelegateTable delegates)
        {
            if (pathPrefixExpression != null)
            {
                body.Assign(
                    failures,
                    body.StaticCall(
                        RuntimeAddNestedFailuresMethod,
                        failures,
                        planExpression,
                        body.Constant(propertyIndex),
                        body.Convert(attemptedValue, typeof(object)),
                        body.Constant(pathPrefix, typeof(string))));
                return;
            }

            if (CanEmitInlineCollectionNested(property, attemptedValue.Type))
            {
                EmitCollectionNestedValidation(body, planExpression, delegatesExpression, failures, attemptedValue, property, propertyIndex, pathPrefix, localPrefix, delegates);
                return;
            }

            if (CanEmitInlineNested(property, attemptedValue.Type))
            {
                body.If(
                    body.Not(body.IsNull(attemptedValue)),
                    branch =>
                    {
                        var nestedPlan = branch.DeclareLocal<ValidationPlan>($"plan_{localPrefix}_nested");
                        branch.Assign(
                            nestedPlan,
                            branch.StaticCall(
                                RuntimeGetNestedPlanMethod,
                                planExpression,
                                branch.Constant(propertyIndex)));

                        EmitProperties(
                            branch,
                            nestedPlan,
                            delegatesExpression,
                            attemptedValue,
                            branch.Convert(attemptedValue, typeof(object)),
                            failures,
                            property.NestedValidation.Plan,
                            delegates,
                            CombinePath(pathPrefix, property.PropertyPath),
                            null,
                            $"{localPrefix}_nested");
                    });

                return;
            }

            body.Assign(
                failures,
                body.StaticCall(
                    RuntimeAddNestedFailuresMethod,
                    failures,
                    planExpression,
                    body.Constant(propertyIndex),
                    body.Convert(attemptedValue, typeof(object)),
                    body.Constant(pathPrefix, typeof(string))));
        }

        private static void EmitCollectionNestedValidation(
            IBeeMethodBodyBuilder body,
            IBeeValueExpression planExpression,
            IBeeValueExpression delegatesExpression,
            IBeeLocal failures,
            IBeeValueExpression attemptedValue,
            PropertyValidationPlan property,
            int propertyIndex,
            string pathPrefix,
            string localPrefix,
            DelegateTable delegates)
        {
            body.If(
                body.Not(body.IsNull(attemptedValue)),
                branch =>
                {
                    var nestedPlan = branch.DeclareLocal<ValidationPlan>($"plan_{localPrefix}_items");
                    var index = branch.DeclareLocal<int>($"index_{localPrefix}");
                    branch.Assign(
                        nestedPlan,
                        branch.StaticCall(
                            RuntimeGetNestedPlanMethod,
                            planExpression,
                            branch.Constant(propertyIndex)));
                    branch.Assign(index, branch.Constant(0));

                    branch.ForEach(attemptedValue, $"item_{localPrefix}", (item, loop) =>
                    {
                        var itemPrefix = loop.Concat(
                            loop.Constant(CombinePath(pathPrefix, property.PropertyPath), typeof(string)),
                            loop.Constant("[", typeof(string)),
                            loop.Convert(index, typeof(object)),
                            loop.Constant("]", typeof(string)));

                        EmitProperties(
                            loop,
                            nestedPlan,
                            delegatesExpression,
                            item,
                            loop.Convert(item, typeof(object)),
                            failures,
                            property.NestedValidation.Plan,
                            delegates,
                            null,
                            itemPrefix,
                            $"{localPrefix}_item");

                        loop.Assign(index, loop.Add(index, loop.Constant(1)));
                    });
                });
        }

        private static IBeeValueExpression TryEmitTypedCondition(
            IBeeMethodBodyBuilder body,
            IBeeValueExpression delegatesExpression,
            IBeeValueExpression instance,
            PropertyValidationPlan property,
            Type modelType,
            DelegateTable delegates)
        {
            if (property.TypedCondition == null || !modelType.IsVisible || !delegates.ConditionIndexes.TryGetValue(property, out var index))
            {
                return null;
            }

            var delegateType = typeof(Func<,>).MakeGenericType(modelType, typeof(bool));
            if (property.TypedCondition.GetType() != delegateType)
            {
                return null;
            }

            var invoke = delegateType.GetMethod(nameof(Func<object, bool>.Invoke));
            var call = body.Call(
                body.Convert(body.Index(delegatesExpression, body.Constant(index)), delegateType),
                invoke,
                instance);

            return property.IsConditionNegated ? body.Not(call) : call;
        }

        private static IBeeValueExpression TryEmitTypedMustInvalid(
            IBeeMethodBodyBuilder body,
            IBeeValueExpression delegatesExpression,
            IBeeValueExpression value,
            Type valueType,
            RulePlan rule,
            DelegateTable delegates)
        {
            if (!valueType.IsValueType || rule.Predicate == null || !valueType.IsVisible || !delegates.RuleIndexes.TryGetValue(rule, out var index))
            {
                return null;
            }

            var delegateType = typeof(Func<,>).MakeGenericType(valueType, typeof(bool));
            if (rule.Predicate.GetType() != delegateType)
            {
                return null;
            }

            var invoke = delegateType.GetMethod(nameof(Func<object, bool>.Invoke));
            return body.Not(body.Call(
                body.Convert(body.Index(delegatesExpression, body.Constant(index)), delegateType),
                invoke,
                value));
        }

        private static IBeeValueExpression TryEmitInvalidExpression(
            IBeeMethodBodyBuilder body,
            IBeeValueExpression delegatesExpression,
            IBeeValueExpression value,
            Type valueType,
            RulePlan rule,
            DelegateTable delegates)
        {
            if (valueType == typeof(object))
            {
                return null;
            }

            if (rule.Kind == RuleKind.Must)
            {
                return TryEmitTypedMustInvalid(body, delegatesExpression, value, valueType, rule, delegates);
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

        private static bool CanEmitInlineNested(PropertyValidationPlan property, Type attemptedValueType)
            => property.NestedValidation != null
                && !property.NestedValidation.IsCollection
                && attemptedValueType != typeof(object)
                && attemptedValueType.IsVisible
                && property.NestedValidation.Plan.ModelType == attemptedValueType;

        private static bool CanEmitInlineCollectionNested(PropertyValidationPlan property, Type attemptedValueType)
            => property.NestedValidation != null
                && property.NestedValidation.IsCollection
                && attemptedValueType != typeof(object)
                && attemptedValueType.IsVisible
                && property.NestedValidation.Plan.ModelType.IsVisible
                && HasVisibleEnumerableElement(attemptedValueType, property.NestedValidation.Plan.ModelType)
                && CanInlinePlanWithoutNested(property.NestedValidation.Plan);

        private static bool CanInlinePlanWithoutNested(ValidationPlan plan)
        {
            for (var i = 0; i < plan.Properties.Count; i++)
            {
                if (plan.Properties[i].NestedValidation != null)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool HasVisibleEnumerableElement(Type enumerableType, Type elementType)
        {
            if (enumerableType.IsArray)
            {
                return enumerableType.GetElementType() == elementType;
            }

            if (enumerableType.IsGenericType && enumerableType.GetGenericArguments().Length == 1 && enumerableType.GetGenericArguments()[0] == elementType)
            {
                return true;
            }

            var interfaces = enumerableType.GetInterfaces();
            for (var i = 0; i < interfaces.Length; i++)
            {
                var current = interfaces[i];
                if (current.IsGenericType
                    && current.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                    && current.GetGenericArguments()[0] == elementType)
                {
                    return true;
                }
            }

            return false;
        }

        private static IBeeValueExpression AppendPath(
            IBeeMethodBodyBuilder body,
            IBeeValueExpression prefix,
            string suffix)
        {
            if (string.IsNullOrWhiteSpace(suffix))
            {
                return prefix;
            }

            return body.Concat(prefix, body.Constant(".", typeof(string)), body.Constant(suffix, typeof(string)));
        }

        private static string CombinePath(string prefix, string suffix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                return suffix ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(suffix))
            {
                return prefix;
            }

            return $"{prefix}.{suffix}";
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

        private static DelegateTable BuildDelegateTable(ValidationPlan plan)
        {
            var items = new List<Delegate>();
            var conditions = new Dictionary<PropertyValidationPlan, int>();
            var rules = new Dictionary<RulePlan, int>();
            AddPlanDelegates(plan, items, conditions, rules);
            return new DelegateTable(items.ToArray(), conditions, rules);
        }

        private static void AddPlanDelegates(
            ValidationPlan plan,
            List<Delegate> items,
            Dictionary<PropertyValidationPlan, int> conditions,
            Dictionary<RulePlan, int> rules)
        {
            for (var propertyIndex = 0; propertyIndex < plan.Properties.Count; propertyIndex++)
            {
                var property = plan.Properties[propertyIndex];
                if (property.TypedCondition != null)
                {
                    conditions[property] = items.Count;
                    items.Add(property.TypedCondition);
                }

                for (var ruleIndex = 0; ruleIndex < property.Rules.Count; ruleIndex++)
                {
                    var rule = property.Rules[ruleIndex];
                    if (rule.Predicate != null)
                    {
                        rules[rule] = items.Count;
                        items.Add(rule.Predicate);
                    }
                }

                if (property.NestedValidation != null)
                {
                    AddPlanDelegates(property.NestedValidation.Plan, items, conditions, rules);
                }
            }
        }

        private sealed class DelegateTable
        {
            public DelegateTable(
                Delegate[] items,
                IReadOnlyDictionary<PropertyValidationPlan, int> conditionIndexes,
                IReadOnlyDictionary<RulePlan, int> ruleIndexes)
            {
                Items = items;
                ConditionIndexes = conditionIndexes;
                RuleIndexes = ruleIndexes;
            }

            public Delegate[] Items { get; }

            public IReadOnlyDictionary<PropertyValidationPlan, int> ConditionIndexes { get; }

            public IReadOnlyDictionary<RulePlan, int> RuleIndexes { get; }
        }

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
