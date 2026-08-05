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
        /// Requires the value to satisfy a custom predicate.
        /// </summary>
        ICrabRuleBuilder<T, TProperty> Must(Func<TProperty, bool> predicate);

        /// <summary>
        /// Requires the value to satisfy an async custom predicate.
        /// </summary>
        ICrabRuleBuilder<T, TProperty> MustAsync(Func<TProperty, CancellationToken, ValueTask<bool>> predicate);

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

        /// <summary>
        /// Applies a child validator to the property value.
        /// </summary>
        ICrabRuleBuilder<T, TProperty> SetValidator<TChild>(CrabValidator<TChild> validator);

        /// <summary>
        /// Applies a child validator resolved by type to the property value.
        /// </summary>
        ICrabRuleBuilder<T, TProperty> SetValidator<TChildValidator>()
            where TChildValidator : class;

        /// <summary>
        /// Applies a child validator to each item in the property collection.
        /// </summary>
        ICrabRuleBuilder<T, TProperty> RuleForEach<TElement>(CrabValidator<TElement> validator);

        /// <summary>
        /// Applies a child validator resolved by type to each item in the property collection.
        /// </summary>
        ICrabRuleBuilder<T, TProperty> RuleForEach<TElementValidator>()
            where TElementValidator : class;
    }
}
