namespace Crabalidator
{
    /// <summary>
    /// Fluent builder for validation rules.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    /// <typeparam name="TProperty">The property type.</typeparam>
    public interface ICrabRuleBuilder<T, TProperty>
    {
        /// <summary>
        /// Requires the value to be non-null.
        /// </summary>
        ICrabRuleBuilder<T, TProperty> NotNull();

        /// <summary>
        /// Requires the value to be non-empty.
        /// </summary>
        ICrabRuleBuilder<T, TProperty> NotEmpty();

        /// <summary>
        /// Requires the value to equal the expected value.
        /// </summary>
        ICrabRuleBuilder<T, TProperty> Equal(TProperty value);

        /// <summary>
        /// Requires the value to differ from the expected value.
        /// </summary>
        ICrabRuleBuilder<T, TProperty> NotEqual(TProperty value);

        /// <summary>
        /// Requires the value to be greater than the expected value.
        /// </summary>
        ICrabRuleBuilder<T, TProperty> GreaterThan(TProperty value);

        /// <summary>
        /// Requires the value to be greater than or equal to the expected value.
        /// </summary>
        ICrabRuleBuilder<T, TProperty> GreaterThanOrEqualTo(TProperty value);

        /// <summary>
        /// Requires the value to be less than the expected value.
        /// </summary>
        ICrabRuleBuilder<T, TProperty> LessThan(TProperty value);

        /// <summary>
        /// Requires the value to be less than or equal to the expected value.
        /// </summary>
        ICrabRuleBuilder<T, TProperty> LessThanOrEqualTo(TProperty value);

        /// <summary>
        /// Requires the value length to be within the specified range.
        /// </summary>
        ICrabRuleBuilder<T, TProperty> Length(int minimum, int maximum);

        /// <summary>
        /// Requires the value length to be at least the specified value.
        /// </summary>
        ICrabRuleBuilder<T, TProperty> MinimumLength(int minimum);

        /// <summary>
        /// Requires the value length to be no more than the specified value.
        /// </summary>
        ICrabRuleBuilder<T, TProperty> MaximumLength(int maximum);

        /// <summary>
        /// Requires the value to satisfy a custom predicate.
        /// </summary>
        ICrabRuleBuilder<T, TProperty> Must(Func<TProperty, bool> predicate);

        /// <summary>
        /// Overrides the error message for the most recently added rule.
        /// </summary>
        ICrabRuleBuilder<T, TProperty> WithMessage(string message);

        /// <summary>
        /// Sets the error code for the most recently added rule.
        /// </summary>
        ICrabRuleBuilder<T, TProperty> WithErrorCode(string errorCode);

        /// <summary>
        /// Sets the severity for the most recently added rule.
        /// </summary>
        ICrabRuleBuilder<T, TProperty> WithSeverity(ValidationSeverity severity);

        /// <summary>
        /// Applies a condition to the property rule.
        /// </summary>
        ICrabRuleBuilder<T, TProperty> When(Func<T, bool> condition);

        /// <summary>
        /// Applies an inverse condition to the property rule.
        /// </summary>
        ICrabRuleBuilder<T, TProperty> Unless(Func<T, bool> condition);

        /// <summary>
        /// Sets cascade behavior for the property rule.
        /// </summary>
        ICrabRuleBuilder<T, TProperty> Cascade(CascadeMode cascadeMode);
    }
}
