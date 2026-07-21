using System.Collections;

namespace Crabalidator.Configuration
{
    /// <summary>
    /// Describes a configured validation rule.
    /// </summary>
    public sealed class RuleDescriptor
    {
        private readonly Func<object, bool> _isValid;

        private RuleDescriptor(
            RuleKind kind,
            string errorMessage,
            Func<object, bool> isValid,
            object comparisonValue = null,
            int? minimum = null,
            int? maximum = null)
        {
            Kind = kind;
            ErrorMessage = errorMessage ?? throw new ArgumentNullException(nameof(errorMessage));
            _isValid = isValid ?? throw new ArgumentNullException(nameof(isValid));
            ComparisonValue = comparisonValue;
            Minimum = minimum;
            Maximum = maximum;
            Severity = ValidationSeverity.Error;
        }

        /// <summary>
        /// Gets the rule kind.
        /// </summary>
        public RuleKind Kind { get; }

        /// <summary>
        /// Gets the default error message.
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// Gets the optional error code.
        /// </summary>
        public string ErrorCode { get; private set; }

        /// <summary>
        /// Gets the failure severity.
        /// </summary>
        public ValidationSeverity Severity { get; private set; }

        /// <summary>
        /// Gets the comparison value when the rule uses one.
        /// </summary>
        public object ComparisonValue { get; }

        /// <summary>
        /// Gets the minimum bound when the rule uses one.
        /// </summary>
        public int? Minimum { get; }

        /// <summary>
        /// Gets the maximum bound when the rule uses one.
        /// </summary>
        public int? Maximum { get; }

        internal static RuleDescriptor NotNull(string propertyName)
            => new RuleDescriptor(
                RuleKind.NotNull,
                $"'{propertyName}' must not be null.",
                value => value != null);

        internal static RuleDescriptor NotEmpty(string propertyName, Type propertyType)
            => new RuleDescriptor(
                RuleKind.NotEmpty,
                $"'{propertyName}' must not be empty.",
                value => !IsEmpty(value, propertyType));

        internal static RuleDescriptor Equal(string propertyName, object expected)
            => new RuleDescriptor(
                RuleKind.Equal,
                $"'{propertyName}' must be equal to '{expected}'.",
                value => object.Equals(value, expected),
                expected);

        internal static RuleDescriptor NotEqual(string propertyName, object expected)
            => new RuleDescriptor(
                RuleKind.NotEqual,
                $"'{propertyName}' must not be equal to '{expected}'.",
                value => !object.Equals(value, expected),
                expected);

        internal static RuleDescriptor Comparison(string propertyName, RuleKind kind, object expected)
        {
            return new RuleDescriptor(
                kind,
                CreateComparisonMessage(propertyName, kind, expected),
                value => Compare(value, expected, kind),
                expected);
        }

        internal static RuleDescriptor Length(string propertyName, int minimum, int maximum)
        {
            EnsureValidRange(minimum, maximum);

            return new RuleDescriptor(
                RuleKind.Length,
                $"'{propertyName}' length must be between {minimum} and {maximum}.",
                value => value == null || IsLengthBetween(value, minimum, maximum),
                minimum: minimum,
                maximum: maximum);
        }

        internal static RuleDescriptor MinimumLength(string propertyName, int minimum)
        {
            EnsureNonNegative(minimum, nameof(minimum));

            return new RuleDescriptor(
                RuleKind.MinimumLength,
                $"'{propertyName}' length must be at least {minimum}.",
                value => value == null || GetLength(value) >= minimum,
                minimum: minimum);
        }

        internal static RuleDescriptor MaximumLength(string propertyName, int maximum)
        {
            EnsureNonNegative(maximum, nameof(maximum));

            return new RuleDescriptor(
                RuleKind.MaximumLength,
                $"'{propertyName}' length must be no more than {maximum}.",
                value => value == null || GetLength(value) <= maximum,
                maximum: maximum);
        }

        internal static RuleDescriptor Must(string propertyName, Func<object, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return new RuleDescriptor(
                RuleKind.Must,
                $"'{propertyName}' is not valid.",
                value => predicate(value));
        }

        internal RuleDescriptor WithMessage(string message)
        {
            ErrorMessage = string.IsNullOrWhiteSpace(message)
                ? throw new ArgumentException(nameof(message))
                : message;
            return this;
        }

        internal RuleDescriptor WithErrorCode(string errorCode)
        {
            ErrorCode = string.IsNullOrWhiteSpace(errorCode)
                ? throw new ArgumentException(nameof(errorCode))
                : errorCode;
            return this;
        }

        internal RuleDescriptor WithSeverity(ValidationSeverity severity)
        {
            Severity = severity;
            return this;
        }

        internal bool IsValid(object value)
            => _isValid(value);

        private static bool IsEmpty(object value, Type propertyType)
        {
            if (value == null)
            {
                return true;
            }

            if (value is string text)
            {
                return string.IsNullOrWhiteSpace(text);
            }

            if (value is ICollection collection)
            {
                return collection.Count == 0;
            }

            if (value is IEnumerable enumerable)
            {
                var enumerator = enumerable.GetEnumerator();
                try
                {
                    return !enumerator.MoveNext();
                }
                finally
                {
                    (enumerator as IDisposable)?.Dispose();
                }
            }

            if (propertyType.IsValueType)
            {
                return object.Equals(value, Activator.CreateInstance(propertyType));
            }

            return false;
        }

        private static bool Compare(object value, object expected, RuleKind kind)
        {
            if (value == null || expected == null)
            {
                return false;
            }

            if (value is not IComparable comparable)
            {
                throw new InvalidOperationException($"Type '{value.GetType().FullName}' is not comparable.");
            }

            var comparison = comparable.CompareTo(expected);

            return kind switch
            {
                RuleKind.GreaterThan => comparison > 0,
                RuleKind.GreaterThanOrEqualTo => comparison >= 0,
                RuleKind.LessThan => comparison < 0,
                RuleKind.LessThanOrEqualTo => comparison <= 0,
                _ => throw new InvalidOperationException($"Unsupported comparison rule '{kind}'.")
            };
        }

        private static int GetLength(object value)
        {
            if (value == null)
            {
                return 0;
            }

            if (value is string text)
            {
                return text.Length;
            }

            if (value is ICollection collection)
            {
                return collection.Count;
            }

            throw new InvalidOperationException($"Type '{value.GetType().FullName}' does not expose a supported length.");
        }

        private static bool IsLengthBetween(object value, int minimum, int maximum)
        {
            var length = GetLength(value);
            return length >= minimum && length <= maximum;
        }

        private static string CreateComparisonMessage(string propertyName, RuleKind kind, object expected)
        {
            var phrase = kind switch
            {
                RuleKind.GreaterThan => "greater than",
                RuleKind.GreaterThanOrEqualTo => "greater than or equal to",
                RuleKind.LessThan => "less than",
                RuleKind.LessThanOrEqualTo => "less than or equal to",
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
            };

            return $"'{propertyName}' must be {phrase} '{expected}'.";
        }

        private static void EnsureValidRange(int minimum, int maximum)
        {
            EnsureNonNegative(minimum, nameof(minimum));
            EnsureNonNegative(maximum, nameof(maximum));

            if (minimum > maximum)
            {
                throw new ArgumentOutOfRangeException(nameof(minimum), "Minimum length cannot exceed maximum length.");
            }
        }

        private static void EnsureNonNegative(int value, string name)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(name, "Length bounds must be non-negative.");
            }
        }
    }
}
