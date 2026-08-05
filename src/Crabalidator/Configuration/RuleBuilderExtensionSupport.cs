namespace Crabalidator.Configuration
{
    internal static class RuleBuilderExtensionSupport
    {
        public static ICrabRuleBuilder<T, TProperty> AddKnownRule<T, TProperty>(
            ICrabRuleBuilder<T, TProperty> builder,
            Func<string, Type, RuleDescriptor> createRule)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (createRule == null)
            {
                throw new ArgumentNullException(nameof(createRule));
            }

            if (builder is not IKnownRuleBuilder<T, TProperty> knownRuleBuilder)
            {
                throw new InvalidOperationException("This rule builder does not support known Crabalidator rules.");
            }

            return knownRuleBuilder.AddKnownRule(createRule(
                knownRuleBuilder.PropertyName,
                knownRuleBuilder.PropertyType));
        }

        public static string GetPropertyName<T, TProperty>(ICrabRuleBuilder<T, TProperty> builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (builder is IKnownRuleBuilder<T, TProperty> knownRuleBuilder)
            {
                return knownRuleBuilder.PropertyName;
            }

            return "Value";
        }
    }
}
