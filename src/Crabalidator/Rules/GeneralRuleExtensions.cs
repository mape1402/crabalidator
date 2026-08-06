using Crabalidator.Configuration;

namespace Crabalidator
{
    /// <summary>
    /// General validation rule extensions available for any property type.
    /// </summary>
    public static class GeneralRuleExtensions
    {
        /// <summary>
        /// Requires the value to be non-null.
        /// </summary>
        public static ICrabRuleBuilder<T, TProperty> NotNull<T, TProperty>(
            this ICrabRuleBuilder<T, TProperty> builder)
            => RuleBuilderExtensionSupport.AddKnownRule(
                builder,
                (propertyName, _) => RuleDescriptor.NotNull(propertyName));

        /// <summary>
        /// Requires the value to be null.
        /// </summary>
        public static ICrabRuleBuilder<T, TProperty> Null<T, TProperty>(
            this ICrabRuleBuilder<T, TProperty> builder)
        {
            var propertyName = RuleBuilderExtensionSupport.GetPropertyName(builder);
            return builder
                .Must(NullRulePredicate<TProperty>.IsNull)
                .WithMessage($"'{propertyName}' must be null.");
        }

        /// <summary>
        /// Requires the value to equal the expected value.
        /// </summary>
        public static ICrabRuleBuilder<T, TProperty> Equal<T, TProperty>(
            this ICrabRuleBuilder<T, TProperty> builder,
            TProperty value)
            => RuleBuilderExtensionSupport.AddKnownRule(
                builder,
                (propertyName, _) => RuleDescriptor.Equal(propertyName, value));

        /// <summary>
        /// Requires the value to differ from the expected value.
        /// </summary>
        public static ICrabRuleBuilder<T, TProperty> NotEqual<T, TProperty>(
            this ICrabRuleBuilder<T, TProperty> builder,
            TProperty value)
            => RuleBuilderExtensionSupport.AddKnownRule(
                builder,
                (propertyName, _) => RuleDescriptor.NotEqual(propertyName, value));

        /// <summary>
        /// Requires the value to be the default value for its type.
        /// </summary>
        public static ICrabRuleBuilder<T, TProperty> Default<T, TProperty>(
            this ICrabRuleBuilder<T, TProperty> builder)
        {
            var propertyName = RuleBuilderExtensionSupport.GetPropertyName(builder);
            return builder
                .Must(NullRulePredicate<TProperty>.IsDefault)
                .WithMessage($"'{propertyName}' must be the default value.");
        }

        /// <summary>
        /// Requires the value to differ from the default value for its type.
        /// </summary>
        public static ICrabRuleBuilder<T, TProperty> NotDefault<T, TProperty>(
            this ICrabRuleBuilder<T, TProperty> builder)
        {
            var propertyName = RuleBuilderExtensionSupport.GetPropertyName(builder);
            return builder
                .Must(NullRulePredicate<TProperty>.IsNotDefault)
                .WithMessage($"'{propertyName}' must not be the default value.");
        }

        /// <summary>
        /// Requires the value to be one of the allowed values.
        /// </summary>
        public static ICrabRuleBuilder<T, TProperty> In<T, TProperty>(
            this ICrabRuleBuilder<T, TProperty> builder,
            params TProperty[] allowedValues)
        {
            if (allowedValues == null)
            {
                throw new ArgumentNullException(nameof(allowedValues));
            }

            var propertyName = RuleBuilderExtensionSupport.GetPropertyName(builder);
            var predicate = new MembershipRulePredicate<TProperty>(allowedValues);
            return builder
                .Must(predicate.Contains)
                .WithMessage($"'{propertyName}' must be one of the allowed values.");
        }

        /// <summary>
        /// Requires the value not to be one of the disallowed values.
        /// </summary>
        public static ICrabRuleBuilder<T, TProperty> NotIn<T, TProperty>(
            this ICrabRuleBuilder<T, TProperty> builder,
            params TProperty[] disallowedValues)
        {
            if (disallowedValues == null)
            {
                throw new ArgumentNullException(nameof(disallowedValues));
            }

            var propertyName = RuleBuilderExtensionSupport.GetPropertyName(builder);
            var predicate = new MembershipRulePredicate<TProperty>(disallowedValues);
            return builder
                .Must(predicate.DoesNotContain)
                .WithMessage($"'{propertyName}' must not be one of the disallowed values.");
        }
    }
}
