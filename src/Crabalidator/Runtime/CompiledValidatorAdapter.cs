namespace Crabalidator.Runtime
{
    /// <summary>
    /// Adapts configured validators to compiled validator execution.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    internal sealed class CompiledValidatorAdapter<T> : IValidator<T>, IAsyncValidator<T>
    {
        private readonly CrabValidator<T> _validator;
        private readonly ICompiledValidatorRegistry _registry;
        private readonly Lazy<TypedCompiledValidator<T>> _compiledValidator;

        public CompiledValidatorAdapter(
            CrabValidator<T> validator,
            ICompiledValidatorRegistry registry)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _compiledValidator = new Lazy<TypedCompiledValidator<T>>(() => TypedCompiledValidator<T>.Create(_registry.GetOrAdd(_validator)));
        }

        public ValidationResult Validate(T instance)
            => Validate(new ValidationContext<T>(instance));

        public ValidationResult Validate(ValidationContext<T> context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return _compiledValidator.Value.Validate(context.InstanceToValidate);
        }

        public ValueTask<ValidationResult> ValidateAsync(T instance, CancellationToken cancellationToken = default)
            => ValidateAsync(new ValidationContext<T>(instance), cancellationToken);

        public ValueTask<ValidationResult> ValidateAsync(ValidationContext<T> context, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new ValueTask<ValidationResult>(Validate(context));
        }
    }
}
