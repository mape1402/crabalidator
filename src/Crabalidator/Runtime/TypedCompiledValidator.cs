using Crabalidator.Generation;

namespace Crabalidator.Runtime
{
    internal sealed class TypedCompiledValidator<T>
    {
        private TypedCompiledValidator(CompiledValidator compiledValidator, Func<T, ValidationResult> validate)
        {
            CompiledValidator = compiledValidator ?? throw new ArgumentNullException(nameof(compiledValidator));
            Validate = validate ?? throw new ArgumentNullException(nameof(validate));
        }

        public CompiledValidator CompiledValidator { get; }

        public Func<T, ValidationResult> Validate { get; }

        public static TypedCompiledValidator<T> Create(CompiledValidator compiledValidator)
        {
            if (compiledValidator.TypedInvoker is ICompiledValidatorInvoker<T> typedInvoker)
            {
                return new TypedCompiledValidator<T>(compiledValidator, typedInvoker.Invoke);
            }

            return new TypedCompiledValidator<T>(compiledValidator, instance => compiledValidator.Invoker.Invoke(instance));
        }
    }
}
