using Crabalidator.Configuration;

namespace Crabalidator.Planning
{
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Builds validation plans from descriptors.
    /// </summary>
    public sealed class ValidationPlanBuilder : IValidationPlanBuilder
    {
        private readonly IServiceProvider _services;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationPlanBuilder"/> class.
        /// </summary>
        public ValidationPlanBuilder()
        {
        }

        internal ValidationPlanBuilder(IServiceProvider services)
        {
            _services = services;
        }

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
                        rule.Predicate,
                        rule.IsValid,
                        rule.IsValidAsync));
                }

                properties.Add(new PropertyValidationPlan(
                    propertyIndex,
                    propertyRule.PropertyName,
                    propertyRule.PropertyPath,
                    propertyRule.PropertyType,
                    propertyRule.CascadeMode,
                    propertyRule.HasCondition,
                    propertyRule.TypedCondition,
                    propertyRule.IsConditionNegated,
                    rules,
                    CreateNestedValidationPlan(descriptor, propertyRule),
                    propertyRule.GetValue,
                    propertyRule.ShouldValidate));
            }

            return new ValidationPlan(
                descriptor.ValidatorType,
                descriptor.ModelType,
                properties,
                diagnostics);
        }

        private NestedValidationPlan CreateNestedValidationPlan(ValidatorDescriptor descriptor, PropertyRuleDescriptor propertyRule)
        {
            var nested = propertyRule.NestedValidator;
            if (nested == null)
            {
                return null;
            }

            var validator = ResolveNestedValidator(descriptor, nested);
            var plan = Build(validator.Descriptor);

            return new NestedValidationPlan(
                nested.ModelType,
                nested.IsCollection,
                plan.RequiresAsync,
                plan,
                value => value == null ? ValidationResult.Success : ValidationPlanExecutor.Execute(plan, value),
                (value, cancellationToken) => value == null
                    ? new ValueTask<ValidationResult>(ValidationResult.Success)
                    : ValidationPlanExecutor.ExecuteAsync(plan, value, cancellationToken));
        }

        private ICrabValidatorDescriptorSource ResolveNestedValidator(ValidatorDescriptor descriptor, NestedValidatorDescriptor nested)
        {
            if (nested.Validator != null)
            {
                return nested.Validator;
            }

            var validator = _services?.GetService(nested.ValidatorType);
            if (validator == null)
            {
                if (nested.ValidatorType.IsAbstract)
                {
                    validator = ResolveDefaultNestedValidator(descriptor, nested);
                }
                else
                {
                    validator = _services == null
                        ? Activator.CreateInstance(nested.ValidatorType)
                        : ActivatorUtilities.CreateInstance(_services, nested.ValidatorType);
                }
            }

            if (validator is not ICrabValidatorDescriptorSource descriptorSource)
            {
                throw new InvalidOperationException($"Type '{nested.ValidatorType.FullName}' must derive from CrabValidator<T>.");
            }

            return descriptorSource;
        }

        private object ResolveDefaultNestedValidator(ValidatorDescriptor descriptor, NestedValidatorDescriptor nested)
        {
            var serviceType = nested.ValidatorType;
            var validator = _services?.GetService(serviceType);
            if (validator != null)
            {
                return validator;
            }

            var candidates = descriptor.ValidatorType.Assembly
                .GetTypes()
                .Where(x => x != null && !x.IsAbstract && !x.IsInterface && !x.ContainsGenericParameters)
                .Where(serviceType.IsAssignableFrom)
                .ToArray();

            if (candidates.Length == 1)
            {
                return _services == null
                    ? Activator.CreateInstance(candidates[0])
                    : ActivatorUtilities.CreateInstance(_services, candidates[0]);
            }

            if (candidates.Length == 0)
            {
                throw new InvalidOperationException(
                    $"No validator was found for nested model '{nested.ModelType.FullName}'. Register a CrabValidator<{nested.ModelType.Name}> with AddCrabalidator or use an explicit nested validator type or instance.");
            }

            throw new InvalidOperationException(
                $"Multiple validators were found for nested model '{nested.ModelType.FullName}'. Use an explicit nested validator type or instance.");
        }
    }
}
