using System.Linq.Expressions;

namespace Crabalidator.Testing
{
    /// <summary>
    /// Assertion helpers for validation results.
    /// </summary>
    public static class ValidationResultAssertionExtensions
    {
        /// <summary>
        /// Asserts that validation succeeded.
        /// </summary>
        /// <param name="result">The validation result.</param>
        /// <returns>The same validation result.</returns>
        public static ValidationResult ShouldBeValid(this ValidationResult result)
        {
            EnsureResult(result);
            if (!result.IsValid)
            {
                throw new CrabalidatorAssertionException(
                    $"Expected validation to succeed, but found {result.Errors.Count} error(s): {FormatFailures(result.Errors)}");
            }

            return result;
        }

        /// <summary>
        /// Asserts that validation failed.
        /// </summary>
        /// <param name="result">The validation result.</param>
        /// <returns>The same validation result.</returns>
        public static ValidationResult ShouldBeInvalid(this ValidationResult result)
        {
            EnsureResult(result);
            if (result.IsValid)
            {
                throw new CrabalidatorAssertionException("Expected validation to fail, but it succeeded.");
            }

            return result;
        }

        /// <summary>
        /// Asserts that validation has an error for a property.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <typeparam name="TProperty">The property type.</typeparam>
        /// <param name="result">The validation result.</param>
        /// <param name="expression">The property expression.</param>
        /// <returns>Assertions scoped to the matching property failures.</returns>
        public static ValidationFailureAssertions ShouldHaveErrorFor<T, TProperty>(
            this ValidationResult result,
            Expression<Func<T, TProperty>> expression)
            => result.ShouldHaveErrorFor(GetPropertyPath(expression));

        /// <summary>
        /// Asserts that validation has an error for a property.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="result">The validation result.</param>
        /// <param name="expression">The property expression.</param>
        /// <returns>Assertions scoped to the matching property failures.</returns>
        public static ValidationFailureAssertions ShouldHaveErrorFor<T>(
            this ValidationResult result,
            Expression<Func<T, object>> expression)
            => result.ShouldHaveErrorFor(GetPropertyPath(expression));

        /// <summary>
        /// Asserts that validation has an error for a property.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <typeparam name="TProperty">The property type.</typeparam>
        /// <param name="result">The typed validation test result.</param>
        /// <param name="expression">The property expression.</param>
        /// <returns>Assertions scoped to the matching property failures.</returns>
        public static ValidationFailureAssertions ShouldHaveErrorFor<T, TProperty>(
            this ValidationTestResult<T> result,
            Expression<Func<T, TProperty>> expression)
        {
            EnsureTestResult(result);
            return result.Result.ShouldHaveErrorFor(expression);
        }

        /// <summary>
        /// Asserts that validation succeeded.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="result">The typed validation test result.</param>
        /// <returns>The same typed validation test result.</returns>
        public static ValidationTestResult<T> ShouldBeValid<T>(this ValidationTestResult<T> result)
        {
            EnsureTestResult(result);
            result.Result.ShouldBeValid();
            return result;
        }

        /// <summary>
        /// Asserts that validation failed.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="result">The typed validation test result.</param>
        /// <returns>The same typed validation test result.</returns>
        public static ValidationTestResult<T> ShouldBeInvalid<T>(this ValidationTestResult<T> result)
        {
            EnsureTestResult(result);
            result.Result.ShouldBeInvalid();
            return result;
        }

        /// <summary>
        /// Asserts that validation has an error for a property path.
        /// </summary>
        /// <param name="result">The validation result.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <returns>Assertions scoped to the matching property failures.</returns>
        public static ValidationFailureAssertions ShouldHaveErrorFor(
            this ValidationResult result,
            string propertyPath)
        {
            EnsureResult(result);
            if (string.IsNullOrWhiteSpace(propertyPath))
            {
                throw new ArgumentException("A property path is required.", nameof(propertyPath));
            }

            var failures = result.Errors
                .Where(x => string.Equals(x.PropertyPath, propertyPath, StringComparison.Ordinal)
                    || string.Equals(x.PropertyName, propertyPath, StringComparison.Ordinal))
                .ToArray();

            if (failures.Length == 0)
            {
                throw new CrabalidatorAssertionException(
                    $"Expected an error for '{propertyPath}', but found: {FormatFailures(result.Errors)}");
            }

            return new ValidationFailureAssertions(failures);
        }

        /// <summary>
        /// Asserts that validation has a specific error message.
        /// </summary>
        /// <param name="result">The validation result.</param>
        /// <param name="message">The expected error message.</param>
        /// <returns>The same validation result.</returns>
        public static ValidationResult ShouldHaveErrorMessage(this ValidationResult result, string message)
        {
            EnsureResult(result);
            if (!result.Errors.Any(x => string.Equals(x.ErrorMessage, message, StringComparison.Ordinal)))
            {
                throw new CrabalidatorAssertionException(
                    $"Expected error message '{message}', but found: {FormatFailures(result.Errors)}");
            }

            return result;
        }

        /// <summary>
        /// Asserts that validation has a specific error message.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="result">The typed validation test result.</param>
        /// <param name="message">The expected error message.</param>
        /// <returns>The same typed validation test result.</returns>
        public static ValidationTestResult<T> ShouldHaveErrorMessage<T>(this ValidationTestResult<T> result, string message)
        {
            EnsureTestResult(result);
            result.Result.ShouldHaveErrorMessage(message);
            return result;
        }

        /// <summary>
        /// Asserts that validation has a specific error code.
        /// </summary>
        /// <param name="result">The validation result.</param>
        /// <param name="errorCode">The expected error code.</param>
        /// <returns>The same validation result.</returns>
        public static ValidationResult ShouldHaveErrorCode(this ValidationResult result, string errorCode)
        {
            EnsureResult(result);
            if (!result.Errors.Any(x => string.Equals(x.ErrorCode, errorCode, StringComparison.Ordinal)))
            {
                throw new CrabalidatorAssertionException(
                    $"Expected error code '{errorCode}', but found: {FormatFailures(result.Errors)}");
            }

            return result;
        }

        /// <summary>
        /// Asserts that validation has a specific error code.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="result">The typed validation test result.</param>
        /// <param name="errorCode">The expected error code.</param>
        /// <returns>The same typed validation test result.</returns>
        public static ValidationTestResult<T> ShouldHaveErrorCode<T>(this ValidationTestResult<T> result, string errorCode)
        {
            EnsureTestResult(result);
            result.Result.ShouldHaveErrorCode(errorCode);
            return result;
        }

        /// <summary>
        /// Asserts that validation has a failure with a specific severity.
        /// </summary>
        /// <param name="result">The validation result.</param>
        /// <param name="severity">The expected severity.</param>
        /// <returns>The same validation result.</returns>
        public static ValidationResult ShouldHaveSeverity(this ValidationResult result, ValidationSeverity severity)
        {
            EnsureResult(result);
            if (!result.Errors.Any(x => x.Severity == severity))
            {
                throw new CrabalidatorAssertionException(
                    $"Expected severity '{severity}', but found: {FormatFailures(result.Errors)}");
            }

            return result;
        }

        /// <summary>
        /// Asserts that validation has a failure with a specific severity.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="result">The typed validation test result.</param>
        /// <param name="severity">The expected severity.</param>
        /// <returns>The same typed validation test result.</returns>
        public static ValidationTestResult<T> ShouldHaveSeverity<T>(this ValidationTestResult<T> result, ValidationSeverity severity)
        {
            EnsureTestResult(result);
            result.Result.ShouldHaveSeverity(severity);
            return result;
        }

        private static void EnsureResult(ValidationResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }
        }

        private static void EnsureTestResult<T>(ValidationTestResult<T> result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }
        }

        private static string GetPropertyPath<T, TProperty>(Expression<Func<T, TProperty>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var names = new Stack<string>();
            var current = StripConvert(expression.Body);
            while (current is MemberExpression member)
            {
                names.Push(member.Member.Name);
                current = StripConvert(member.Expression);
            }

            if (names.Count == 0)
            {
                throw new ArgumentException("Expression must point to a property.", nameof(expression));
            }

            return string.Join(".", names);
        }

        private static Expression StripConvert(Expression expression)
        {
            while (expression is UnaryExpression unary
                && (unary.NodeType == ExpressionType.Convert || unary.NodeType == ExpressionType.ConvertChecked))
            {
                expression = unary.Operand;
            }

            return expression;
        }

        internal static string FormatFailures(IEnumerable<ValidationFailure> failures)
        {
            var formatted = (failures ?? Array.Empty<ValidationFailure>())
                .Select(x => $"{x.PropertyPath}: {x.ErrorMessage} [{x.ErrorCode ?? "no-code"}, {x.Severity}]")
                .ToArray();

            return formatted.Length == 0 ? "no errors" : string.Join("; ", formatted);
        }
    }
}
