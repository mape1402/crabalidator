namespace Crabalidator
{
    /// <summary>
    /// Represents the outcome of validation.
    /// </summary>
    public sealed class ValidationResult
    {
        private static readonly ValidationResult ValidResult = new ValidationResult(Array.Empty<ValidationFailure>());

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationResult"/> class.
        /// </summary>
        /// <param name="errors">The validation failures.</param>
        public ValidationResult(IEnumerable<ValidationFailure> errors)
            : this((errors ?? Array.Empty<ValidationFailure>()).ToArray())
        {
        }

        private ValidationResult(IReadOnlyList<ValidationFailure> errors)
        {
            Errors = errors ?? Array.Empty<ValidationFailure>();
        }

        /// <summary>
        /// Gets a successful validation result.
        /// </summary>
        public static ValidationResult Success => ValidResult;

        /// <summary>
        /// Gets a value indicating whether validation succeeded.
        /// </summary>
        public bool IsValid => Errors.Count == 0;

        /// <summary>
        /// Gets the validation failures.
        /// </summary>
        public IReadOnlyList<ValidationFailure> Errors { get; }

        /// <summary>
        /// Creates a validation result from failures.
        /// </summary>
        /// <param name="errors">The validation failures.</param>
        /// <returns>A validation result.</returns>
        public static ValidationResult FromFailures(IEnumerable<ValidationFailure> errors)
            => new ValidationResult(errors);

        internal static ValidationResult FromFailureList(List<ValidationFailure> errors)
            => new ValidationResult(errors);
    }
}
