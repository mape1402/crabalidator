using System.Collections;
using Crabalidator.Configuration;

namespace Crabalidator
{
    /// <summary>
    /// Collection validation rule extensions.
    /// </summary>
    public static class CollectionRuleExtensions
    {
        /// <summary>
        /// Requires the collection to be non-null and contain at least one item.
        /// </summary>
        public static ICrabRuleBuilder<T, TCollection> NotEmpty<T, TCollection>(
            this ICrabRuleBuilder<T, TCollection> builder)
            where TCollection : IEnumerable
            => RuleBuilderExtensionSupport.AddKnownRule(
                builder,
                (propertyName, propertyType) => RuleDescriptor.NotEmpty(propertyName, propertyType));

        /// <summary>
        /// Requires the collection to be null or contain no items.
        /// </summary>
        public static ICrabRuleBuilder<T, TCollection> Empty<T, TCollection>(
            this ICrabRuleBuilder<T, TCollection> builder)
            where TCollection : IEnumerable
        {
            var propertyName = RuleBuilderExtensionSupport.GetPropertyName(builder);
            var predicate = new CollectionCountRulePredicate<TCollection>(0, 0);
            return builder
                .Must(predicate.IsEmpty)
                .WithMessage($"'{propertyName}' must be empty.");
        }

        /// <summary>
        /// Requires the collection count to equal the expected value.
        /// Null values are allowed; combine with NotNull when required.
        /// </summary>
        public static ICrabRuleBuilder<T, TCollection> Count<T, TCollection>(
            this ICrabRuleBuilder<T, TCollection> builder,
            int count)
            where TCollection : IEnumerable
        {
            EnsureNonNegative(count, nameof(count));
            var propertyName = RuleBuilderExtensionSupport.GetPropertyName(builder);
            var predicate = new CollectionCountRulePredicate<TCollection>(count, count);

            return builder
                .Must(predicate.HasCount)
                .WithMessage($"'{propertyName}' must contain {count} item(s).");
        }

        /// <summary>
        /// Requires the collection count to be at least the specified value.
        /// Null values are allowed; combine with NotNull when required.
        /// </summary>
        public static ICrabRuleBuilder<T, TCollection> MinimumCount<T, TCollection>(
            this ICrabRuleBuilder<T, TCollection> builder,
            int minimum)
            where TCollection : IEnumerable
        {
            EnsureNonNegative(minimum, nameof(minimum));
            var propertyName = RuleBuilderExtensionSupport.GetPropertyName(builder);
            var predicate = new CollectionCountRulePredicate<TCollection>(minimum, minimum);

            return builder
                .Must(predicate.HasMinimumCount)
                .WithMessage($"'{propertyName}' must contain at least {minimum} item(s).");
        }

        /// <summary>
        /// Requires the collection count to be no more than the specified value.
        /// Null values are allowed; combine with NotNull when required.
        /// </summary>
        public static ICrabRuleBuilder<T, TCollection> MaximumCount<T, TCollection>(
            this ICrabRuleBuilder<T, TCollection> builder,
            int maximum)
            where TCollection : IEnumerable
        {
            EnsureNonNegative(maximum, nameof(maximum));
            var propertyName = RuleBuilderExtensionSupport.GetPropertyName(builder);
            var predicate = new CollectionCountRulePredicate<TCollection>(maximum, maximum);

            return builder
                .Must(predicate.HasMaximumCount)
                .WithMessage($"'{propertyName}' must contain no more than {maximum} item(s).");
        }

        /// <summary>
        /// Requires the collection count to be within the specified inclusive range.
        /// Null values are allowed; combine with NotNull when required.
        /// </summary>
        public static ICrabRuleBuilder<T, TCollection> CountBetween<T, TCollection>(
            this ICrabRuleBuilder<T, TCollection> builder,
            int minimum,
            int maximum)
            where TCollection : IEnumerable
        {
            EnsureRange(minimum, maximum);
            var propertyName = RuleBuilderExtensionSupport.GetPropertyName(builder);
            var predicate = new CollectionCountRulePredicate<TCollection>(minimum, maximum);

            return builder
                .Must(predicate.IsBetween)
                .WithMessage($"'{propertyName}' must contain between {minimum} and {maximum} item(s).");
        }

        private static int GetCount(IEnumerable enumerable)
        {
            if (enumerable is ICollection collection)
            {
                return collection.Count;
            }

            var count = 0;
            var enumerator = enumerable.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    count++;
                }
            }
            finally
            {
                (enumerator as IDisposable)?.Dispose();
            }

            return count;
        }

        private static void EnsureRange(int minimum, int maximum)
        {
            EnsureNonNegative(minimum, nameof(minimum));
            EnsureNonNegative(maximum, nameof(maximum));

            if (minimum > maximum)
            {
                throw new ArgumentOutOfRangeException(nameof(minimum), "Minimum count cannot exceed maximum count.");
            }
        }

        private static void EnsureNonNegative(int value, string name)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(name, "Count bounds must be non-negative.");
            }
        }
    }
}
