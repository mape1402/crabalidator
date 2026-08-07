namespace Crabalidator.Testing
{
    /// <summary>
    /// Runs a specific validator directly.
    /// </summary>
    /// <typeparam name="TValidator">The validator type.</typeparam>
    public sealed class CrabalidatorTestRunner<TValidator>
        where TValidator : class
    {
        /// <summary>
        /// Validates an instance asynchronously.
        /// </summary>
        /// <typeparam name="TModel">The model type.</typeparam>
        /// <param name="instance">The instance to validate.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The validation result.</returns>
        public async ValueTask<ValidationTestResult<TModel>> ValidateAsync<TModel>(
            TModel instance,
            CancellationToken cancellationToken = default)
        {
            var validator = CreateValidator<TModel>();
            var result = await validator.ValidateAsync(instance, cancellationToken);
            return new ValidationTestResult<TModel>(result);
        }

        /// <summary>
        /// Validates an instance synchronously.
        /// </summary>
        /// <typeparam name="TModel">The model type.</typeparam>
        /// <param name="instance">The instance to validate.</param>
        /// <returns>The validation result.</returns>
        public ValidationTestResult<TModel> Validate<TModel>(TModel instance)
        {
            var validator = CreateValidator<TModel>();
            return new ValidationTestResult<TModel>(validator.Validate(instance));
        }

        private static CrabValidator<TModel> CreateValidator<TModel>()
        {
            var validator = Activator.CreateInstance(typeof(TValidator)) as CrabValidator<TModel>;
            if (validator == null)
            {
                throw new InvalidOperationException(
                    $"Validator '{typeof(TValidator).FullName}' must derive from CrabValidator<{typeof(TModel).Name}> and expose a parameterless constructor for direct testing.");
            }

            return validator;
        }
    }
}
