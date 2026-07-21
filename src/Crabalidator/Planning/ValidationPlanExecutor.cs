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

            if (plan.RequiresAsync)
            {
                throw new InvalidOperationException("This validation plan contains async rules and must be executed with ValidateAsync.");
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

            return Execute(plan, context.InstanceToValidate);
        }

        public static ValueTask<ValidationResult> ExecuteAsync(
            ValidationPlan plan,
            object instance,
            CancellationToken cancellationToken = default)
        {
            if (plan == null)
            {
                throw new ArgumentNullException(nameof(plan));
            }

            return ExecuteAsyncCore(plan, instance, cancellationToken);
        }

        public static ValueTask<ValidationResult> ExecuteAsync<T>(
            ValidationPlan plan,
            ValidationContext<T> context,
            CancellationToken cancellationToken = default)
        {
            if (plan == null)
            {
                throw new ArgumentNullException(nameof(plan));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return ExecuteAsyncCore(plan, context.InstanceToValidate, cancellationToken);
        }

        private static ValidationResult ExecuteCore(ValidationPlan plan, object instance)
        {
            List<ValidationFailure> failures = null;
            failures = ExecuteInto(plan, instance, null, failures);
            return failures == null ? ValidationResult.Success : ValidationResult.FromFailureList(failures);
        }

        private static List<ValidationFailure> ExecuteInto(
            ValidationPlan plan,
            object instance,
            string pathPrefix,
            List<ValidationFailure> failures)
        {
            for (var propertyIndex = 0; propertyIndex < plan.Properties.Count; propertyIndex++)
            {
                var property = plan.Properties[propertyIndex];
                if (!property.ShouldValidate(instance))
                {
                    continue;
                }

                var attemptedValue = property.GetValue(instance);

                for (var ruleIndex = 0; ruleIndex < property.Rules.Count; ruleIndex++)
                {
                    var rule = property.Rules[ruleIndex];
                    if (rule.IsValid(attemptedValue))
                    {
                        continue;
                    }

                    failures ??= new List<ValidationFailure>();
                    failures.Add(new ValidationFailure(
                        rule.Failure.PropertyName,
                        CombinePath(pathPrefix, rule.Failure.PropertyPath),
                        rule.Failure.ErrorMessage,
                        attemptedValue,
                        rule.Failure.ErrorCode,
                        rule.Failure.Severity));

                    if (property.CascadeMode == CascadeMode.Stop)
                    {
                        break;
                    }
                }

                failures = AddNestedFailures(failures, property, attemptedValue, pathPrefix);
            }

            return failures;
        }

        private static async ValueTask<ValidationResult> ExecuteAsyncCore(
            ValidationPlan plan,
            object instance,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            List<ValidationFailure> failures = null;
            failures = await ExecuteIntoAsync(plan, instance, null, failures, cancellationToken).ConfigureAwait(false);
            return failures == null ? ValidationResult.Success : ValidationResult.FromFailureList(failures);
        }

        private static async ValueTask<List<ValidationFailure>> ExecuteIntoAsync(
            ValidationPlan plan,
            object instance,
            string pathPrefix,
            List<ValidationFailure> failures,
            CancellationToken cancellationToken)
        {
            for (var propertyIndex = 0; propertyIndex < plan.Properties.Count; propertyIndex++)
            {
                var property = plan.Properties[propertyIndex];
                cancellationToken.ThrowIfCancellationRequested();

                if (!property.ShouldValidate(instance))
                {
                    continue;
                }

                var attemptedValue = property.GetValue(instance);

                for (var ruleIndex = 0; ruleIndex < property.Rules.Count; ruleIndex++)
                {
                    var rule = property.Rules[ruleIndex];
                    if (await rule.IsValidAsync(attemptedValue, cancellationToken).ConfigureAwait(false))
                    {
                        continue;
                    }

                    failures ??= new List<ValidationFailure>();
                    failures.Add(new ValidationFailure(
                        rule.Failure.PropertyName,
                        CombinePath(pathPrefix, rule.Failure.PropertyPath),
                        rule.Failure.ErrorMessage,
                        attemptedValue,
                        rule.Failure.ErrorCode,
                        rule.Failure.Severity));

                    if (property.CascadeMode == CascadeMode.Stop)
                    {
                        break;
                    }
                }

                failures = await AddNestedFailuresAsync(failures, property, attemptedValue, pathPrefix, cancellationToken).ConfigureAwait(false);
            }

            return failures;
        }

        private static List<ValidationFailure> AddNestedFailures(
            List<ValidationFailure> failures,
            PropertyValidationPlan property,
            object attemptedValue,
            string pathPrefix)
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
                    var itemPrefix = CombinePath(pathPrefix, $"{property.PropertyPath}[{index}]");
                    failures = ExecuteInto(nested.Plan, item, itemPrefix, failures);
                    index++;
                }

                return failures;
            }

            return ExecuteInto(nested.Plan, attemptedValue, CombinePath(pathPrefix, property.PropertyPath), failures);
        }

        private static async ValueTask<List<ValidationFailure>> AddNestedFailuresAsync(
            List<ValidationFailure> failures,
            PropertyValidationPlan property,
            object attemptedValue,
            string pathPrefix,
            CancellationToken cancellationToken)
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
                    cancellationToken.ThrowIfCancellationRequested();
                    failures = await ExecuteIntoAsync(
                        nested.Plan,
                        item,
                        CombinePath(pathPrefix, $"{property.PropertyPath}[{index}]"),
                        failures,
                        cancellationToken).ConfigureAwait(false);
                    index++;
                }

                return failures;
            }

            return await ExecuteIntoAsync(
                nested.Plan,
                attemptedValue,
                CombinePath(pathPrefix, property.PropertyPath),
                failures,
                cancellationToken).ConfigureAwait(false);
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
