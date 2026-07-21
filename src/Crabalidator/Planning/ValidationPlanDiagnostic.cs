namespace Crabalidator.Planning
{
    /// <summary>
    /// Describes a validation plan diagnostic.
    /// </summary>
    public sealed class ValidationPlanDiagnostic
    {
        private ValidationPlanDiagnostic(
            string code,
            ValidationPlanDiagnosticSeverity severity,
            string message,
            string propertyPath)
        {
            Code = code ?? throw new ArgumentNullException(nameof(code));
            Severity = severity;
            Message = message ?? throw new ArgumentNullException(nameof(message));
            PropertyPath = propertyPath;
        }

        /// <summary>
        /// Gets the diagnostic code.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Gets the diagnostic severity.
        /// </summary>
        public ValidationPlanDiagnosticSeverity Severity { get; }

        /// <summary>
        /// Gets the diagnostic message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the optional property path.
        /// </summary>
        public string PropertyPath { get; }

        /// <summary>
        /// Creates a warning diagnostic.
        /// </summary>
        /// <param name="code">The diagnostic code.</param>
        /// <param name="message">The diagnostic message.</param>
        /// <param name="propertyPath">The optional property path.</param>
        /// <returns>The diagnostic.</returns>
        public static ValidationPlanDiagnostic Warning(string code, string message, string propertyPath = null)
            => new ValidationPlanDiagnostic(code, ValidationPlanDiagnosticSeverity.Warning, message, propertyPath);
    }
}
