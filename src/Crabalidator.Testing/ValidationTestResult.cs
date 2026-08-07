namespace Crabalidator.Testing
{
    /// <summary>
    /// Wraps a validation result with the validated model type for typed assertions.
    /// </summary>
    /// <typeparam name="T">The validated model type.</typeparam>
    public sealed class ValidationTestResult<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationTestResult{T}"/> class.
        /// </summary>
        /// <param name="result">The validation result.</param>
        public ValidationTestResult(ValidationResult result)
        {
            Result = result ?? throw new ArgumentNullException(nameof(result));
        }

        /// <summary>
        /// Gets the underlying validation result.
        /// </summary>
        public ValidationResult Result { get; }

        /// <summary>
        /// Gets a value indicating whether validation succeeded.
        /// </summary>
        public bool IsValid => Result.IsValid;

        /// <summary>
        /// Gets the validation failures.
        /// </summary>
        public IReadOnlyList<ValidationFailure> Errors => Result.Errors;

        /// <summary>
        /// Converts a typed test result to its underlying validation result.
        /// </summary>
        /// <param name="result">The typed test result.</param>
        public static implicit operator ValidationResult(ValidationTestResult<T> result)
            => result?.Result;
    }
}
