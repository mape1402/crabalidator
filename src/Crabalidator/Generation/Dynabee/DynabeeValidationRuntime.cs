using Crabalidator.Planning;

namespace Crabalidator.Generation.Dynabee
{
    /// <summary>
    /// Runtime helpers used by validators emitted through DynaBee.
    /// </summary>
    public static class DynabeeValidationRuntime
    {
        /// <summary>
        /// Executes a planned property condition.
        /// </summary>
        public static bool ShouldValidate(ValidationPlan plan, int propertyIndex, object instance)
            => plan.Properties[propertyIndex].ShouldValidate(instance);

        /// <summary>
        /// Reads a planned property value through the compiled accessor.
        /// </summary>
        public static object GetValue(ValidationPlan plan, int propertyIndex, object instance)
            => plan.Properties[propertyIndex].GetValue(instance);

        /// <summary>
        /// Gets a nested validation plan for emitted recursive validation.
        /// </summary>
        public static ValidationPlan GetNestedPlan(ValidationPlan plan, int propertyIndex)
            => plan.Properties[propertyIndex].NestedValidation.Plan;

        /// <summary>
        /// Executes a planned rule when the IL backend cannot inline it directly.
        /// </summary>
        public static bool IsRuleInvalid(ValidationPlan plan, int propertyIndex, int ruleIndex, object value)
            => !plan.Properties[propertyIndex].Rules[ruleIndex].IsValid(value);

        /// <summary>
        /// Appends a validation failure for an emitted validator.
        /// </summary>
        public static List<ValidationFailure> AddFailure(
            List<ValidationFailure> failures,
            ValidationPlan plan,
            int propertyIndex,
            int ruleIndex,
            object attemptedValue,
            string pathPrefix)
        {
            var rule = plan.Properties[propertyIndex].Rules[ruleIndex];
            failures ??= new List<ValidationFailure>();
            failures.Add(new ValidationFailure(
                rule.Failure.PropertyName,
                CombinePath(pathPrefix, rule.Failure.PropertyPath),
                rule.Failure.ErrorMessage,
                attemptedValue,
                rule.Failure.ErrorCode,
                rule.Failure.Severity));

            return failures;
        }

        /// <summary>
        /// Appends nested validation failures for an emitted validator.
        /// </summary>
        public static List<ValidationFailure> AddNestedFailures(
            List<ValidationFailure> failures,
            ValidationPlan plan,
            int propertyIndex,
            object attemptedValue,
            string pathPrefix)
            => ValidationPlanExecutor.AddNestedFailures(plan, propertyIndex, attemptedValue, pathPrefix, failures);

        /// <summary>
        /// Creates the final validation result from the lazily allocated failure list.
        /// </summary>
        public static ValidationResult ToResult(List<ValidationFailure> failures)
            => failures == null ? ValidationResult.Success : ValidationResult.FromFailureList(failures);

        private static string CombinePath(string prefix, string suffix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                return suffix ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(suffix))
            {
                return prefix;
            }

            return $"{prefix}.{suffix}";
        }
    }
}
