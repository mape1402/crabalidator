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
    }
}
