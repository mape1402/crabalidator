using Crabalidator.Configuration;

namespace Crabalidator.Planning
{
    /// <summary>
    /// Builds validation plans from descriptors.
    /// </summary>
    public sealed class ValidationPlanBuilder : IValidationPlanBuilder
    {
        /// <inheritdoc/>
        public ValidationPlan Build(ValidatorDescriptor descriptor)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            var diagnostics = new List<ValidationPlanDiagnostic>();

            if (descriptor.Rules.Count == 0)
            {
                diagnostics.Add(ValidationPlanDiagnostic.Warning(
                    "CRABPLAN001",
                    "Validator does not contain any property rules."));
            }

            var properties = new List<PropertyValidationPlan>(descriptor.Rules.Count);

            for (var propertyIndex = 0; propertyIndex < descriptor.Rules.Count; propertyIndex++)
            {
                var propertyRule = descriptor.Rules[propertyIndex];
                var rules = new List<RulePlan>(propertyRule.Rules.Count);

                if (propertyRule.Rules.Count == 0 && propertyRule.NestedValidator == null)
                {
                    diagnostics.Add(ValidationPlanDiagnostic.Warning(
                        "CRABPLAN002",
                        $"Property '{propertyRule.PropertyPath}' does not contain validation rules.",
                        propertyRule.PropertyPath));
                }

                for (var ruleIndex = 0; ruleIndex < propertyRule.Rules.Count; ruleIndex++)
                {
                    var rule = propertyRule.Rules[ruleIndex];
                    rules.Add(new RulePlan(
                        ruleIndex,
                        rule.Kind,
                        rule.ComparisonValue,
                        rule.Minimum,
                        rule.Maximum,
                        new FailurePlan(
                            propertyRule.PropertyName,
                            propertyRule.PropertyPath,
                            rule.ErrorMessage,
                            rule.ErrorCode,
                            rule.Severity),
                        rule.IsAsync,
                        false,
                        rule.IsValid,
                        rule.IsValidAsync));
                }

                properties.Add(new PropertyValidationPlan(
                    propertyIndex,
                    propertyRule.PropertyName,
                    propertyRule.PropertyPath,
                    propertyRule.PropertyType,
                    propertyRule.CascadeMode,
                    rules,
                    CreateNestedValidationPlan(propertyRule),
                    propertyRule.GetValue,
                    propertyRule.ShouldValidate));
            }

            return new ValidationPlan(
                descriptor.ValidatorType,
                descriptor.ModelType,
                properties,
                diagnostics);
        }

        private static NestedValidationPlan CreateNestedValidationPlan(PropertyRuleDescriptor propertyRule)
        {
            var nested = propertyRule.NestedValidator;
            if (nested == null)
            {
                return null;
            }

            return new NestedValidationPlan(
                nested.ModelType,
                nested.IsCollection,
                nested.IsAsync,
                nested.Plan,
                nested.Validate,
                nested.ValidateAsync);
        }
    }
}
