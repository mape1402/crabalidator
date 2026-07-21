namespace Crabalidator
{
    /// <summary>
    /// Validates instances.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    public interface IValidator<T>
    {
        /// <summary>
        /// Validates an instance.
        /// </summary>
        /// <param name="instance">The instance to validate.</param>
        /// <returns>The validation result.</returns>
        ValidationResult Validate(T instance);

        /// <summary>
        /// Validates a context.
        /// </summary>
        /// <param name="context">The validation context.</param>
        /// <returns>The validation result.</returns>
        ValidationResult Validate(ValidationContext<T> context);
    }
}
