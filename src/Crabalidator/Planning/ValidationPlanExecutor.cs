namespace Crabalidator.Planning
{
    internal static class ValidationPlanExecutor
    {
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

            List<ValidationFailure> failures = null;

            foreach (var property in plan.Properties)
            {
                var attemptedValue = property.GetValue(context.InstanceToValidate);

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
                }
            }

            return failures == null ? ValidationResult.Success : ValidationResult.FromFailures(failures);
        }
    }
}
