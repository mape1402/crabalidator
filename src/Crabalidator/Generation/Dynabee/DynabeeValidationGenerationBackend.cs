using System.Reflection;
using DynaBee;
using DynaBee.FluentApi;
using DynaBee.FluentApi.DependencyInjection;
using DynaBee.FluentApi.Invocation;
using Crabalidator.Planning;

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
                        .EmitsBody(body =>
                        {
                            var planExpression = body.Property(body.Self(), PlanPropertyName);
                            var instance = body.Parameter("instance");
                            body.Return(body.Call(
                                planExpression,
                                ValidationPlanExecuteMethod,
                                body.Convert(instance, typeof(object))));
                        })))
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
