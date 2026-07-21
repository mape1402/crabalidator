namespace Crabalidator
{
    /// <summary>
    /// Validates instances asynchronously.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    public interface IAsyncValidator<T>
    {
        /// <summary>
        /// Validates an instance asynchronously.
        /// </summary>
        /// <param name="instance">The instance to validate.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The validation result.</returns>
        ValueTask<ValidationResult> ValidateAsync(T instance, CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates a context asynchronously.
        /// </summary>
        /// <param name="context">The validation context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The validation result.</returns>
        ValueTask<ValidationResult> ValidateAsync(ValidationContext<T> context, CancellationToken cancellationToken = default);
    }
}
