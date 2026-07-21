namespace Crabalidator.Configuration
{
    /// <summary>
    /// Identifies built-in rule kinds.
    /// </summary>
    public enum RuleKind
    {
        /// <summary>
        /// Non-null rule.
        /// </summary>
        NotNull,

        /// <summary>
        /// Non-empty rule.
        /// </summary>
        NotEmpty,

        /// <summary>
        /// Equality rule.
        /// </summary>
        Equal,

        /// <summary>
        /// Inequality rule.
        /// </summary>
        NotEqual,

        /// <summary>
        /// Greater-than rule.
        /// </summary>
        GreaterThan,

        /// <summary>
        /// Greater-than-or-equal rule.
        /// </summary>
        GreaterThanOrEqualTo,

        /// <summary>
        /// Less-than rule.
        /// </summary>
        LessThan,

        /// <summary>
        /// Less-than-or-equal rule.
        /// </summary>
        LessThanOrEqualTo,

        /// <summary>
        /// Exact range length rule.
        /// </summary>
        Length,

        /// <summary>
        /// Minimum length rule.
        /// </summary>
        MinimumLength,

        /// <summary>
        /// Maximum length rule.
        /// </summary>
        MaximumLength,

        /// <summary>
        /// Custom predicate rule.
        /// </summary>
        Must,

        /// <summary>
        /// Async custom predicate rule.
        /// </summary>
        MustAsync
    }
}
