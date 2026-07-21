using System.Text;
using Crabalidator.Planning;

namespace Crabalidator.Diagnostics
{
    /// <summary>
    /// Diagnostic helpers for validation plans.
    /// </summary>
    public static class ValidationPlanExtensions
    {
        /// <summary>
        /// Describes a compiled validation plan.
        /// </summary>
        /// <param name="plan">The validation plan.</param>
        /// <returns>A readable plan description.</returns>
        public static string DescribePlan(this ValidationPlan plan)
        {
            if (plan == null)
            {
                throw new ArgumentNullException(nameof(plan));
            }

            var builder = new StringBuilder();
            builder.AppendLine($"Validator: {plan.ValidatorType.FullName}");
            builder.AppendLine($"Model: {plan.ModelType.FullName}");
            builder.AppendLine($"Requires async: {plan.RequiresAsync}");
            builder.AppendLine($"Requires context: {plan.RequiresContext}");

            if (plan.Diagnostics.Count > 0)
            {
                builder.AppendLine("Diagnostics:");

                foreach (var diagnostic in plan.Diagnostics)
                {
                    var path = string.IsNullOrWhiteSpace(diagnostic.PropertyPath)
                        ? string.Empty
                        : $" ({diagnostic.PropertyPath})";
                    builder.AppendLine($"- {diagnostic.Severity} {diagnostic.Code}{path}: {diagnostic.Message}");
                }
            }

            builder.AppendLine("Properties:");

            foreach (var property in plan.Properties)
            {
                builder.AppendLine($"- #{property.Order} {property.PropertyPath} ({property.PropertyType.Name})");
                builder.AppendLine($"  Cascade: {property.CascadeMode}");

                foreach (var rule in property.Rules)
                {
                    var asyncMarker = rule.IsAsync ? " async" : string.Empty;
                    builder.AppendLine($"  - #{rule.Order} {rule.Kind}{asyncMarker}: {rule.Failure.ErrorMessage}");

                    if (!string.IsNullOrWhiteSpace(rule.Failure.ErrorCode))
                    {
                        builder.AppendLine($"    Error code: {rule.Failure.ErrorCode}");
                    }

                    builder.AppendLine($"    Severity: {rule.Failure.Severity}");
                    AppendRuleBounds(builder, rule);
                }

                if (property.NestedValidation != null)
                {
                    var kind = property.NestedValidation.IsCollection ? "RuleForEach" : "SetValidator";
                    var asyncMarker = property.NestedValidation.IsAsync ? " async" : string.Empty;
                    builder.AppendLine($"  - {kind}({property.NestedValidation.ModelType.Name}){asyncMarker}");
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Describes the validation plan for a configured validator.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="validator">The validator.</param>
        /// <returns>A readable plan description.</returns>
        public static string DescribePlan<T>(this CrabValidator<T> validator)
        {
            if (validator == null)
            {
                throw new ArgumentNullException(nameof(validator));
            }

            return validator.Plan.DescribePlan();
        }

        private static void AppendRuleBounds(StringBuilder builder, RulePlan rule)
        {
            if (rule.ComparisonValue != null)
            {
                builder.AppendLine($"    Comparison value: {rule.ComparisonValue}");
            }

            if (rule.Minimum != null)
            {
                builder.AppendLine($"    Minimum: {rule.Minimum}");
            }

            if (rule.Maximum != null)
            {
                builder.AppendLine($"    Maximum: {rule.Maximum}");
            }
        }
    }
}
