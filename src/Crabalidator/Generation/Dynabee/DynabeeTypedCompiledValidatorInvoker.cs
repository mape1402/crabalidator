namespace Crabalidator.Generation.Dynabee
{
    /// <summary>
    /// Adapts a DynaBee-created typed validator delegate.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    internal sealed class DynabeeTypedCompiledValidatorInvoker<T> : ICompiledValidatorInvoker<T>
    {
        private readonly Func<T, ValidationResult> _validate;

        public DynabeeTypedCompiledValidatorInvoker(Func<T, ValidationResult> validate)
        {
            _validate = validate ?? throw new ArgumentNullException(nameof(validate));
        }

        public ValidationResult Invoke(T instance)
            => _validate(instance);
    }
}
