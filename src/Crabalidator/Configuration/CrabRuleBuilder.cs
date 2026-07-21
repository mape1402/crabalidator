namespace Crabalidator.Configuration
{
    using System.Collections;

    /// <summary>
    /// Fluent builder for property rules.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    /// <typeparam name="TProperty">The property type.</typeparam>
    internal sealed class CrabRuleBuilder<T, TProperty> : ICrabRuleBuilder<T, TProperty>
    {
        private readonly PropertyRuleDescriptor _propertyRule;
        private RuleDescriptor _currentRule;

        public CrabRuleBuilder(PropertyRuleDescriptor propertyRule)
        {
            _propertyRule = propertyRule ?? throw new ArgumentNullException(nameof(propertyRule));
        }

        public ICrabRuleBuilder<T, TProperty> NotNull()
            => Add(RuleDescriptor.NotNull(_propertyRule.PropertyName));

        public ICrabRuleBuilder<T, TProperty> NotEmpty()
            => Add(RuleDescriptor.NotEmpty(_propertyRule.PropertyName, typeof(TProperty)));

        public ICrabRuleBuilder<T, TProperty> Equal(TProperty value)
            => Add(RuleDescriptor.Equal(_propertyRule.PropertyName, value));

        public ICrabRuleBuilder<T, TProperty> NotEqual(TProperty value)
            => Add(RuleDescriptor.NotEqual(_propertyRule.PropertyName, value));

        public ICrabRuleBuilder<T, TProperty> GreaterThan(TProperty value)
            => Add(RuleDescriptor.Comparison(_propertyRule.PropertyName, RuleKind.GreaterThan, value));

        public ICrabRuleBuilder<T, TProperty> GreaterThanOrEqualTo(TProperty value)
            => Add(RuleDescriptor.Comparison(_propertyRule.PropertyName, RuleKind.GreaterThanOrEqualTo, value));

        public ICrabRuleBuilder<T, TProperty> LessThan(TProperty value)
            => Add(RuleDescriptor.Comparison(_propertyRule.PropertyName, RuleKind.LessThan, value));

        public ICrabRuleBuilder<T, TProperty> LessThanOrEqualTo(TProperty value)
            => Add(RuleDescriptor.Comparison(_propertyRule.PropertyName, RuleKind.LessThanOrEqualTo, value));

        public ICrabRuleBuilder<T, TProperty> Length(int minimum, int maximum)
            => Add(RuleDescriptor.Length(_propertyRule.PropertyName, minimum, maximum));

        public ICrabRuleBuilder<T, TProperty> MinimumLength(int minimum)
            => Add(RuleDescriptor.MinimumLength(_propertyRule.PropertyName, minimum));

        public ICrabRuleBuilder<T, TProperty> MaximumLength(int maximum)
            => Add(RuleDescriptor.MaximumLength(_propertyRule.PropertyName, maximum));

        public ICrabRuleBuilder<T, TProperty> Must(Func<TProperty, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return Add(RuleDescriptor.Must(_propertyRule.PropertyName, value => predicate((TProperty)value)));
        }

        public ICrabRuleBuilder<T, TProperty> MustAsync(Func<TProperty, CancellationToken, ValueTask<bool>> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return Add(RuleDescriptor.MustAsync(
                _propertyRule.PropertyName,
                (value, cancellationToken) => predicate((TProperty)value, cancellationToken)));
        }

        public ICrabRuleBuilder<T, TProperty> WithMessage(string message)
            => ConfigureCurrent(rule => rule.WithMessage(message));

        public ICrabRuleBuilder<T, TProperty> WithErrorCode(string errorCode)
            => ConfigureCurrent(rule => rule.WithErrorCode(errorCode));

        public ICrabRuleBuilder<T, TProperty> WithSeverity(ValidationSeverity severity)
            => ConfigureCurrent(rule => rule.WithSeverity(severity));

        public ICrabRuleBuilder<T, TProperty> When(Func<T, bool> condition)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            _propertyRule.SetCondition(instance => condition((T)instance));
            return this;
        }

        public ICrabRuleBuilder<T, TProperty> Unless(Func<T, bool> condition)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            _propertyRule.SetCondition(instance => !condition((T)instance));
            return this;
        }

        public ICrabRuleBuilder<T, TProperty> Cascade(CascadeMode cascadeMode)
        {
            _propertyRule.SetCascadeMode(cascadeMode);
            return this;
        }

        public ICrabRuleBuilder<T, TProperty> SetValidator<TChild>(CrabValidator<TChild> validator)
        {
            if (validator == null)
            {
                throw new ArgumentNullException(nameof(validator));
            }

            if (!typeof(TChild).IsAssignableFrom(typeof(TProperty)))
            {
                throw new InvalidOperationException(
                    $"Validator for '{typeof(TChild).FullName}' cannot validate property '{_propertyRule.PropertyPath}' of type '{typeof(TProperty).FullName}'.");
            }

            _propertyRule.SetNestedValidator(new NestedValidatorDescriptor(
                typeof(TChild),
                false,
                validator.Plan.RequiresAsync,
                validator.Plan,
                value => value == null ? ValidationResult.Success : validator.Plan.Execute(value),
                (value, cancellationToken) => value == null
                    ? new ValueTask<ValidationResult>(ValidationResult.Success)
                    : validator.Plan.ExecuteAsync(value, cancellationToken)));

            return this;
        }

        public ICrabRuleBuilder<T, TProperty> RuleForEach<TElement>(CrabValidator<TElement> validator)
        {
            if (validator == null)
            {
                throw new ArgumentNullException(nameof(validator));
            }

            if (!NestedValidatorDescriptor.IsEnumerableButNotString(typeof(TProperty)))
            {
                throw new InvalidOperationException(
                    $"RuleForEach requires an enumerable property, but '{_propertyRule.PropertyPath}' is '{typeof(TProperty).FullName}'.");
            }

            _propertyRule.SetNestedValidator(new NestedValidatorDescriptor(
                typeof(TElement),
                true,
                validator.Plan.RequiresAsync,
                validator.Plan,
                value => value == null ? ValidationResult.Success : validator.Plan.Execute(value),
                (value, cancellationToken) => value == null
                    ? new ValueTask<ValidationResult>(ValidationResult.Success)
                    : validator.Plan.ExecuteAsync(value, cancellationToken)));

            return this;
        }

        private ICrabRuleBuilder<T, TProperty> Add(RuleDescriptor rule)
        {
            _propertyRule.AddRule(rule);
            _currentRule = rule;
            return this;
        }

        private ICrabRuleBuilder<T, TProperty> ConfigureCurrent(Func<RuleDescriptor, RuleDescriptor> configure)
        {
            if (_currentRule == null)
            {
                throw new InvalidOperationException("No validation rule has been configured for this property.");
            }

            configure(_currentRule);
            return this;
        }
    }
}
