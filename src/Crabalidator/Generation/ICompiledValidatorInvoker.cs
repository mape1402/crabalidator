namespace Crabalidator.Generation
{
    /// <summary>
    /// Invokes a compiled validator through object arguments.
    /// </summary>
    public interface ICompiledValidatorInvoker
    {
        /// <summary>
        /// Invokes validation.
        /// </summary>
        /// <param name="instance">The model instance.</param>
        /// <returns>The validation result.</returns>
        ValidationResult Invoke(object instance);
    }

    /// <summary>
    /// Invokes a compiled validator through a typed fast path.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    public interface ICompiledValidatorInvoker<T>
    {
        /// <summary>
        /// Invokes validation.
        /// </summary>
        /// <param name="instance">The model instance.</param>
        /// <returns>The validation result.</returns>
        ValidationResult Invoke(T instance);
    }
}
