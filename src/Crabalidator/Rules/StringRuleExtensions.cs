using System.Text.RegularExpressions;
using Crabalidator.Configuration;

namespace Crabalidator
{
    /// <summary>
    /// String validation rule extensions.
    /// </summary>
    public static class StringRuleExtensions
    {
        /// <summary>
        /// Requires the string to be non-null and contain non-whitespace text.
        /// </summary>
        public static ICrabRuleBuilder<T, string> NotEmpty<T>(
            this ICrabRuleBuilder<T, string> builder)
            => RuleBuilderExtensionSupport.AddKnownRule(
                builder,
                (propertyName, propertyType) => RuleDescriptor.NotEmpty(propertyName, propertyType));

        /// <summary>
        /// Requires the string to be null, empty, or whitespace.
        /// </summary>
        public static ICrabRuleBuilder<T, string> Empty<T>(
            this ICrabRuleBuilder<T, string> builder)
        {
            var propertyName = RuleBuilderExtensionSupport.GetPropertyName(builder);
            return builder
                .Must(string.IsNullOrWhiteSpace)
                .WithMessage($"'{propertyName}' must be empty.");
        }

        /// <summary>
        /// Requires the string length to be within the specified inclusive range.
        /// Null values are allowed; combine with NotNull or NotEmpty when required.
        /// </summary>
        public static ICrabRuleBuilder<T, string> Length<T>(
            this ICrabRuleBuilder<T, string> builder,
            int minimum,
            int maximum)
            => RuleBuilderExtensionSupport.AddKnownRule(
                builder,
                (propertyName, _) => RuleDescriptor.Length(propertyName, minimum, maximum));

        /// <summary>
        /// Requires the string length to be at least the specified value.
        /// Null values are allowed; combine with NotNull or NotEmpty when required.
        /// </summary>
        public static ICrabRuleBuilder<T, string> MinimumLength<T>(
            this ICrabRuleBuilder<T, string> builder,
            int minimum)
            => RuleBuilderExtensionSupport.AddKnownRule(
                builder,
                (propertyName, _) => RuleDescriptor.MinimumLength(propertyName, minimum));

        /// <summary>
        /// Requires the string length to be no more than the specified value.
        /// Null values are allowed; combine with NotNull or NotEmpty when required.
        /// </summary>
        public static ICrabRuleBuilder<T, string> MaximumLength<T>(
            this ICrabRuleBuilder<T, string> builder,
            int maximum)
            => RuleBuilderExtensionSupport.AddKnownRule(
                builder,
                (propertyName, _) => RuleDescriptor.MaximumLength(propertyName, maximum));

        /// <summary>
        /// Requires the string to match the specified regular expression pattern.
        /// Null values are allowed; combine with NotNull or NotEmpty when required.
        /// </summary>
        public static ICrabRuleBuilder<T, string> Matches<T>(
            this ICrabRuleBuilder<T, string> builder,
            string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
            {
                throw new ArgumentException("A regular expression pattern is required.", nameof(pattern));
            }

            var propertyName = RuleBuilderExtensionSupport.GetPropertyName(builder);
            var predicate = new StringPatternRulePredicate(pattern);
            return builder
                .Must(predicate.IsMatch)
                .WithMessage($"'{propertyName}' is not in the correct format.");
        }

        /// <summary>
        /// Requires the string to match the specified regular expression.
        /// Null values are allowed; combine with NotNull or NotEmpty when required.
        /// </summary>
        public static ICrabRuleBuilder<T, string> Matches<T>(
            this ICrabRuleBuilder<T, string> builder,
            Regex regex)
        {
            if (regex == null)
            {
                throw new ArgumentNullException(nameof(regex));
            }

            var propertyName = RuleBuilderExtensionSupport.GetPropertyName(builder);
            var predicate = new StringPatternRulePredicate(regex);
            return builder
                .Must(predicate.IsMatch)
                .WithMessage($"'{propertyName}' is not in the correct format.");
        }

        /// <summary>
        /// Requires the string to look like an email address.
        /// Null values are allowed; combine with NotNull or NotEmpty when required.
        /// </summary>
        public static ICrabRuleBuilder<T, string> EmailAddress<T>(
            this ICrabRuleBuilder<T, string> builder)
        {
            var propertyName = RuleBuilderExtensionSupport.GetPropertyName(builder);
            return builder
                .Must(EmailAddressRulePredicate.IsValid)
                .WithMessage($"'{propertyName}' must be a valid email address.");
        }

        /// <summary>
        /// Requires the string to start with the specified value.
        /// Null values are allowed; combine with NotNull or NotEmpty when required.
        /// </summary>
        public static ICrabRuleBuilder<T, string> StartsWith<T>(
            this ICrabRuleBuilder<T, string> builder,
            string value,
            StringComparison comparison = StringComparison.Ordinal)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var propertyName = RuleBuilderExtensionSupport.GetPropertyName(builder);
            var predicate = new StringValueRulePredicate(value, comparison);
            return builder
                .Must(predicate.StartsWith)
                .WithMessage($"'{propertyName}' must start with '{value}'.");
        }

        /// <summary>
        /// Requires the string to end with the specified value.
        /// Null values are allowed; combine with NotNull or NotEmpty when required.
        /// </summary>
        public static ICrabRuleBuilder<T, string> EndsWith<T>(
            this ICrabRuleBuilder<T, string> builder,
            string value,
            StringComparison comparison = StringComparison.Ordinal)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var propertyName = RuleBuilderExtensionSupport.GetPropertyName(builder);
            var predicate = new StringValueRulePredicate(value, comparison);
            return builder
                .Must(predicate.EndsWith)
                .WithMessage($"'{propertyName}' must end with '{value}'.");
        }

        /// <summary>
        /// Requires the string to contain the specified value.
        /// Null values are allowed; combine with NotNull or NotEmpty when required.
        /// </summary>
        public static ICrabRuleBuilder<T, string> Contains<T>(
            this ICrabRuleBuilder<T, string> builder,
            string value,
            StringComparison comparison = StringComparison.Ordinal)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var propertyName = RuleBuilderExtensionSupport.GetPropertyName(builder);
            var predicate = new StringValueRulePredicate(value, comparison);
            return builder
                .Must(predicate.Contains)
                .WithMessage($"'{propertyName}' must contain '{value}'.");
        }
    }
}
