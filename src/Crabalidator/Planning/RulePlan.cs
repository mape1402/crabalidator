using Crabalidator.Configuration;

namespace Crabalidator.Planning
{
    /// <summary>
    /// Describes a planned validation rule.
    /// </summary>
    public sealed class RulePlan
    {
        private readonly Func<object, bool> _isValid;

        /// <summary>
        /// Initializes a new instance of the <see cref="RulePlan"/> class.
        /// </summary>
        /// <param name="order">The rule order within its property.</param>
        /// <param name="kind">The rule kind.</param>
        /// <param name="comparisonValue">The optional comparison value.</param>
        /// <param name="minimum">The optional minimum value.</param>
        /// <param name="maximum">The optional maximum value.</param>
        /// <param name="failure">The failure plan.</param>
        /// <param name="isAsync">A value indicating whether the rule is async-only.</param>
        /// <param name="requiresContext">A value indicating whether the rule needs validation context.</param>
        /// <param name="isValid">The configured predicate.</param>
        internal RulePlan(
            int order,
            RuleKind kind,
            object comparisonValue,
            int? minimum,
            int? maximum,
            FailurePlan failure,
            bool isAsync,
            bool requiresContext,
            Func<object, bool> isValid)
        {
            Order = order;
            Kind = kind;
            ComparisonValue = comparisonValue;
            Minimum = minimum;
            Maximum = maximum;
            Failure = failure ?? throw new ArgumentNullException(nameof(failure));
            IsAsync = isAsync;
            RequiresContext = requiresContext;
            _isValid = isValid ?? throw new ArgumentNullException(nameof(isValid));
        }

        /// <summary>
        /// Gets the rule order within its property.
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// Gets the rule kind.
        /// </summary>
        public RuleKind Kind { get; }

        /// <summary>
        /// Gets the optional comparison value.
        /// </summary>
        public object ComparisonValue { get; }

        /// <summary>
        /// Gets the optional minimum value.
        /// </summary>
        public int? Minimum { get; }

        /// <summary>
        /// Gets the optional maximum value.
        /// </summary>
        public int? Maximum { get; }

        /// <summary>
        /// Gets the failure plan.
        /// </summary>
        public FailurePlan Failure { get; }

        /// <summary>
        /// Gets a value indicating whether the rule is async-only.
        /// </summary>
        public bool IsAsync { get; }

        /// <summary>
        /// Gets a value indicating whether the rule requires validation context.
        /// </summary>
        public bool RequiresContext { get; }

        internal bool IsValid(object value)
            => _isValid(value);
    }
}
