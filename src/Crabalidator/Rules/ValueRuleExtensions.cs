using Crabalidator.Configuration;

namespace Crabalidator
{
    /// <summary>
    /// Value-type validation rule extensions.
    /// </summary>
    public static class ValueRuleExtensions
    {
        /// <summary>
        /// Requires the value to differ from the default value for its type.
        /// </summary>
        public static ICrabRuleBuilder<T, TProperty> NotEmpty<T, TProperty>(
            this ICrabRuleBuilder<T, TProperty> builder)
            where TProperty : struct
            => RuleBuilderExtensionSupport.AddKnownRule(
                builder,
                (propertyName, propertyType) => RuleDescriptor.NotEmpty(propertyName, propertyType));

        /// <summary>
        /// Requires the value to be the default value for its type.
        /// </summary>
        public static ICrabRuleBuilder<T, TProperty> Empty<T, TProperty>(
            this ICrabRuleBuilder<T, TProperty> builder)
            where TProperty : struct
        {
            var propertyName = RuleBuilderExtensionSupport.GetPropertyName(builder);
            return builder
                .Must(value => EqualityComparer<TProperty>.Default.Equals(value, default))
                .WithMessage($"'{propertyName}' must be empty.");
        }
    }
}
