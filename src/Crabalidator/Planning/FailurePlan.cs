namespace Crabalidator.Planning
{
    /// <summary>
    /// Describes the validation failure produced by a rule.
    /// </summary>
    public sealed class FailurePlan
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FailurePlan"/> class.
        /// </summary>
        /// <param name="propertyName">The terminal property name.</param>
        /// <param name="propertyPath">The full property path.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="severity">The failure severity.</param>
        public FailurePlan(
            string propertyName,
            string propertyPath,
            string errorMessage,
            string errorCode,
            ValidationSeverity severity)
        {
            PropertyName = propertyName ?? string.Empty;
            PropertyPath = propertyPath ?? PropertyName;
            ErrorMessage = errorMessage ?? string.Empty;
            ErrorCode = errorCode;
            Severity = severity;
        }

        /// <summary>
        /// Gets the terminal property name.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Gets the full property path.
        /// </summary>
        public string PropertyPath { get; }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        public string ErrorMessage { get; }

        /// <summary>
        /// Gets the error code.
        /// </summary>
        public string ErrorCode { get; }

        /// <summary>
        /// Gets the failure severity.
        /// </summary>
        public ValidationSeverity Severity { get; }
    }
}
