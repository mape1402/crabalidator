namespace Crabalidator.Testing
{
    /// <summary>
    /// Assertion helpers scoped to selected validation failures.
    /// </summary>
    public sealed class ValidationFailureAssertions
    {
        internal ValidationFailureAssertions(IReadOnlyList<ValidationFailure> failures)
        {
            Failures = failures ?? Array.Empty<ValidationFailure>();
        }

        /// <summary>
        /// Gets the selected failures.
        /// </summary>
        public IReadOnlyList<ValidationFailure> Failures { get; }

        /// <summary>
        /// Asserts that a specific number of failures matched.
        /// </summary>
        /// <param name="count">The expected count.</param>
        /// <returns>The same assertion scope.</returns>
        public ValidationFailureAssertions WithErrorCount(int count)
        {
            if (Failures.Count != count)
            {
                throw new CrabalidatorAssertionException(
                    $"Expected {count} error(s), but found {Failures.Count}: {ValidationResultAssertionExtensions.FormatFailures(Failures)}");
            }

            return this;
        }

        /// <summary>
        /// Asserts that a matching failure has a specific message.
        /// </summary>
        /// <param name="message">The expected message.</param>
        /// <returns>The same assertion scope.</returns>
        public ValidationFailureAssertions WithMessage(string message)
        {
            if (!Failures.Any(x => string.Equals(x.ErrorMessage, message, StringComparison.Ordinal)))
            {
                throw new CrabalidatorAssertionException(
                    $"Expected error message '{message}', but found: {ValidationResultAssertionExtensions.FormatFailures(Failures)}");
            }

            return this;
        }

        /// <summary>
        /// Asserts that a matching failure has a specific error code.
        /// </summary>
        /// <param name="errorCode">The expected error code.</param>
        /// <returns>The same assertion scope.</returns>
        public ValidationFailureAssertions WithErrorCode(string errorCode)
        {
            if (!Failures.Any(x => string.Equals(x.ErrorCode, errorCode, StringComparison.Ordinal)))
            {
                throw new CrabalidatorAssertionException(
                    $"Expected error code '{errorCode}', but found: {ValidationResultAssertionExtensions.FormatFailures(Failures)}");
            }

            return this;
        }

        /// <summary>
        /// Asserts that a matching failure has a specific severity.
        /// </summary>
        /// <param name="severity">The expected severity.</param>
        /// <returns>The same assertion scope.</returns>
        public ValidationFailureAssertions WithSeverity(ValidationSeverity severity)
        {
            if (!Failures.Any(x => x.Severity == severity))
            {
                throw new CrabalidatorAssertionException(
                    $"Expected severity '{severity}', but found: {ValidationResultAssertionExtensions.FormatFailures(Failures)}");
            }

            return this;
        }
    }
}
