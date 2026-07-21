namespace Crabalidator
{
    /// <summary>
    /// Describes a single validation failure.
    /// </summary>
    public sealed class ValidationFailure
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationFailure"/> class.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="attemptedValue">The attempted value.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="severity">The failure severity.</param>
        public ValidationFailure(
            string propertyName,
            string propertyPath,
            string errorMessage,
            object attemptedValue = null,
            string errorCode = null,
            ValidationSeverity severity = ValidationSeverity.Error)
        {
            PropertyName = propertyName ?? string.Empty;
            PropertyPath = propertyPath ?? PropertyName;
            ErrorMessage = errorMessage ?? string.Empty;
            AttemptedValue = attemptedValue;
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
        /// Gets the attempted value.
        /// </summary>
        public object AttemptedValue { get; }

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
