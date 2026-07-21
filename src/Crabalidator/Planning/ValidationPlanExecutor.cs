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

        private static async ValueTask<ValidationResult> ExecuteAsyncCore(
            ValidationPlan plan,
            object instance,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            List<ValidationFailure> failures = null;

            foreach (var property in plan.Properties)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!property.ShouldValidate(instance))
                {
                    continue;
                }

                var attemptedValue = property.GetValue(instance);

                foreach (var rule in property.Rules)
                {
                    if (await rule.IsValidAsync(attemptedValue, cancellationToken).ConfigureAwait(false))
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

                failures = await AddNestedFailuresAsync(failures, property, attemptedValue, cancellationToken).ConfigureAwait(false);
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

        private static async ValueTask<List<ValidationFailure>> AddNestedFailuresAsync(
            List<ValidationFailure> failures,
            PropertyValidationPlan property,
            object attemptedValue,
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
                    failures = AddNestedResultFailures(
                        failures,
                        $"{property.PropertyPath}[{index}]",
                        item,
                        await nested.ValidateAsync(item, cancellationToken).ConfigureAwait(false));
                    index++;
                }

                return failures;
            }

            return AddNestedResultFailures(
                failures,
                property.PropertyPath,
                attemptedValue,
                await nested.ValidateAsync(attemptedValue, cancellationToken).ConfigureAwait(false));
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
