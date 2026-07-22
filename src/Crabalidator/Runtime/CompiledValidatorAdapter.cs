using Crabalidator.Planning;
using Crabalidator.Generation;

namespace Crabalidator.Runtime
{
    /// <summary>
    /// Adapts configured validators to compiled validator execution.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    internal sealed class CompiledValidatorAdapter<T> : IValidator<T>, IAsyncValidator<T>
    {
        private readonly CrabValidator<T> _validator;
        private readonly IServiceProvider _services;
        private readonly IValidationGenerationBackend _generationBackend;
        private readonly Lazy<ValidationPlan> _plan;
        private readonly Lazy<TypedCompiledValidator<T>> _compiledValidator;

        public CompiledValidatorAdapter(
            CrabValidator<T> validator,
            IServiceProvider services,
            IValidationGenerationBackend generationBackend)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _generationBackend = generationBackend ?? throw new ArgumentNullException(nameof(generationBackend));
            _plan = new Lazy<ValidationPlan>(() => new ValidationPlanBuilder(_services).Build(_validator.Descriptor));
            _compiledValidator = new Lazy<TypedCompiledValidator<T>>(() => TypedCompiledValidator<T>.Create(_generationBackend.Compile(_plan.Value)));
        }

        public ValidationResult Validate(T instance)
        {
            if (_plan.Value.RequiresAsync)
            {
                throw new InvalidOperationException("This validator contains async rules and must be executed with ValidateAsync.");
            }

            return _compiledValidator.Value.Validate(instance);
        }

        public ValidationResult Validate(ValidationContext<T> context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (_plan.Value.RequiresAsync)
            {
                throw new InvalidOperationException("This validator contains async rules and must be executed with ValidateAsync.");
            }

            return _compiledValidator.Value.Validate(context.InstanceToValidate);
        }

        public ValueTask<ValidationResult> ValidateAsync(T instance, CancellationToken cancellationToken = default)
        {
            if (_plan.Value.RequiresAsync)
            {
                return ValidationPlanExecutor.ExecuteAsync(_plan.Value, instance, cancellationToken);
            }

            return new ValueTask<ValidationResult>(Validate(instance));
        }

        public ValueTask<ValidationResult> ValidateAsync(ValidationContext<T> context, CancellationToken cancellationToken = default)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (_plan.Value.RequiresAsync)
            {
                return ValidationPlanExecutor.ExecuteAsync(_plan.Value, context, cancellationToken);
            }

            return new ValueTask<ValidationResult>(Validate(context));
        }
    }
}
