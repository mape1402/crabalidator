namespace Crabalidator.Testing
{
    /// <summary>
    /// Direct testing helpers for validators.
    /// </summary>
    public static class DirectValidatorTestingExtensions
    {
        /// <summary>
        /// Validates an instance with a validator and returns a typed test result.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="validator">The validator.</param>
        /// <param name="instance">The instance to validate.</param>
        /// <returns>The typed validation test result.</returns>
        public static ValidationTestResult<T> TestValidate<T>(
            this IValidator<T> validator,
            T instance)
        {
            if (validator == null)
            {
                throw new ArgumentNullException(nameof(validator));
            }

            return new ValidationTestResult<T>(validator.Validate(instance));
        }

        /// <summary>
        /// Validates an instance asynchronously with a validator and returns a typed test result.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="validator">The validator.</param>
        /// <param name="instance">The instance to validate.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The typed validation test result.</returns>
        public static async ValueTask<ValidationTestResult<T>> TestValidateAsync<T>(
            this IAsyncValidator<T> validator,
            T instance,
            CancellationToken cancellationToken = default)
        {
            if (validator == null)
            {
                throw new ArgumentNullException(nameof(validator));
            }

            var result = await validator.ValidateAsync(instance, cancellationToken);
            return new ValidationTestResult<T>(result);
        }
    }
}
