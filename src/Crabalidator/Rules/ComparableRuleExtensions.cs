using Crabalidator.Configuration;

namespace Crabalidator
{
    /// <summary>
    /// Comparable value validation rule extensions.
    /// </summary>
    public static class ComparableRuleExtensions
    {
        /// <summary>
        /// Requires the value to be greater than the expected value.
        /// </summary>
        public static ICrabRuleBuilder<T, TProperty> GreaterThan<T, TProperty>(
            this ICrabRuleBuilder<T, TProperty> builder,
            TProperty value)
            where TProperty : IComparable
            => RuleBuilderExtensionSupport.AddKnownRule(
                builder,
                (propertyName, _) => RuleDescriptor.Comparison(propertyName, RuleKind.GreaterThan, value));

        /// <summary>
        /// Requires the value to be greater than or equal to the expected value.
        /// </summary>
        public static ICrabRuleBuilder<T, TProperty> GreaterThanOrEqualTo<T, TProperty>(
            this ICrabRuleBuilder<T, TProperty> builder,
            TProperty value)
            where TProperty : IComparable
            => RuleBuilderExtensionSupport.AddKnownRule(
                builder,
                (propertyName, _) => RuleDescriptor.Comparison(propertyName, RuleKind.GreaterThanOrEqualTo, value));

        /// <summary>
        /// Requires the value to be less than the expected value.
        /// </summary>
        public static ICrabRuleBuilder<T, TProperty> LessThan<T, TProperty>(
            this ICrabRuleBuilder<T, TProperty> builder,
            TProperty value)
            where TProperty : IComparable
            => RuleBuilderExtensionSupport.AddKnownRule(
                builder,
                (propertyName, _) => RuleDescriptor.Comparison(propertyName, RuleKind.LessThan, value));

        /// <summary>
        /// Requires the value to be less than or equal to the expected value.
        /// </summary>
        public static ICrabRuleBuilder<T, TProperty> LessThanOrEqualTo<T, TProperty>(
            this ICrabRuleBuilder<T, TProperty> builder,
            TProperty value)
            where TProperty : IComparable
            => RuleBuilderExtensionSupport.AddKnownRule(
                builder,
                (propertyName, _) => RuleDescriptor.Comparison(propertyName, RuleKind.LessThanOrEqualTo, value));

        /// <summary>
        /// Requires the value to be inside the inclusive range.
        /// </summary>
        public static ICrabRuleBuilder<T, TProperty> InclusiveBetween<T, TProperty>(
            this ICrabRuleBuilder<T, TProperty> builder,
            TProperty minimum,
            TProperty maximum)
            where TProperty : IComparable
        {
            EnsureRange(minimum, maximum);
            var propertyName = RuleBuilderExtensionSupport.GetPropertyName(builder);
            var predicate = new ComparableRangeRulePredicate<TProperty>(minimum, maximum);

            return builder
                .Must(predicate.IsInclusive)
                .WithMessage($"'{propertyName}' must be between '{minimum}' and '{maximum}'.");
        }

        /// <summary>
        /// Requires the value to be inside the exclusive range.
        /// </summary>
        public static ICrabRuleBuilder<T, TProperty> ExclusiveBetween<T, TProperty>(
            this ICrabRuleBuilder<T, TProperty> builder,
            TProperty minimum,
            TProperty maximum)
            where TProperty : IComparable
        {
            EnsureRange(minimum, maximum);
            var propertyName = RuleBuilderExtensionSupport.GetPropertyName(builder);
            var predicate = new ComparableRangeRulePredicate<TProperty>(minimum, maximum);

            return builder
                .Must(predicate.IsExclusive)
                .WithMessage($"'{propertyName}' must be between '{minimum}' and '{maximum}' exclusive.");
        }

        private static void EnsureRange<TProperty>(TProperty minimum, TProperty maximum)
            where TProperty : IComparable
        {
            if (minimum == null)
            {
                throw new ArgumentNullException(nameof(minimum));
            }

            if (maximum == null)
            {
                throw new ArgumentNullException(nameof(maximum));
            }

            if (minimum.CompareTo(maximum) > 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minimum), "Minimum cannot exceed maximum.");
            }
        }
    }
}
