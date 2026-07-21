namespace Crabalidator.Planning
{
    internal static class ValidationPlanExecutor
    {
        public static ValidationResult Execute(ValidationPlan plan, object instance)
        {
            if (plan == null)
            {
                throw new ArgumentNullException(nameof(plan));
            }

            return ExecuteCore(plan, instance);
        }

        public static ValidationResult Execute<T>(ValidationPlan plan, ValidationContext<T> context)
        {
            if (plan == null)
            {
                throw new ArgumentNullException(nameof(plan));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return ExecuteCore(plan, context.InstanceToValidate);
        }

        private static ValidationResult ExecuteCore(ValidationPlan plan, object instance)
        {
            List<ValidationFailure> failures = null;

            foreach (var property in plan.Properties)
            {
                if (!property.ShouldValidate(instance))
                {
                    continue;
                }

                var attemptedValue = property.GetValue(instance);

                foreach (var rule in property.Rules)
                {
                    if (rule.IsValid(attemptedValue))
                    {
                        continue;
                    }

                    failures ??= new List<ValidationFailure>();
                    failures.Add(new ValidationFailure(
                        rule.Failure.PropertyName,
                        rule.Failure.PropertyPath,
                        rule.Failure.ErrorMessage,
                        attemptedValue,
                        rule.Failure.ErrorCode,
                        rule.Failure.Severity));

                    if (property.CascadeMode == CascadeMode.Stop)
                    {
                        break;
                    }
                }

                failures = AddNestedFailures(failures, property, attemptedValue);
            }

            return failures == null ? ValidationResult.Success : ValidationResult.FromFailures(failures);
        }

        private static List<ValidationFailure> AddNestedFailures(
            List<ValidationFailure> failures,
            PropertyValidationPlan property,
            object attemptedValue)
        {
            var nested = property.NestedValidation;
            if (nested == null || attemptedValue == null)
            {
                return failures;
            }

            if (nested.IsCollection)
            {
                var index = 0;
                foreach (var item in nested.Enumerate(attemptedValue))
                {
                    failures = AddNestedResultFailures(
                        failures,
                        $"{property.PropertyPath}[{index}]",
                        item,
                        nested.Validate(item));
                    index++;
                }

                return failures;
            }

            return AddNestedResultFailures(
                failures,
                property.PropertyPath,
                attemptedValue,
                nested.Validate(attemptedValue));
        }

        private static List<ValidationFailure> AddNestedResultFailures(
            List<ValidationFailure> failures,
            string pathPrefix,
            object attemptedValue,
            ValidationResult result)
        {
            if (result == null || result.IsValid)
            {
                return failures;
            }

            failures ??= new List<ValidationFailure>();

            foreach (var failure in result.Errors)
            {
                failures.Add(new ValidationFailure(
                    failure.PropertyName,
                    CombinePath(pathPrefix, failure.PropertyPath),
                    failure.ErrorMessage,
                    failure.AttemptedValue ?? attemptedValue,
                    failure.ErrorCode,
                    failure.Severity));
            }

            return failures;
        }

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
