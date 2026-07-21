namespace Crabalidator
{
    /// <summary>
    /// Resolves and executes validators for model instances.
    /// </summary>
    public interface ICrabalidator
    {
        /// <summary>
        /// Validates an instance using the registered validator for its type.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="instance">The instance to validate.</param>
        /// <returns>The validation result.</returns>
        ValidationResult Validate<T>(T instance);

        /// <summary>
        /// Validates an instance asynchronously using the registered validator for its type.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="instance">The instance to validate.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The validation result.</returns>
        ValueTask<ValidationResult> ValidateAsync<T>(T instance, CancellationToken cancellationToken = default);
    }
}
