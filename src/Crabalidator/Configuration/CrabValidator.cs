using System.Linq.Expressions;
using Crabalidator.Configuration;
using Crabalidator.Planning;

namespace Crabalidator
{
    /// <summary>
    /// Base class for configuring validators.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    public abstract class CrabValidator<T> : IValidator<T>, IAsyncValidator<T>, ICrabValidatorDescriptorSource
    {
        private readonly ValidatorDescriptor _descriptor;
        private readonly Lazy<ValidationPlan> _plan;

        /// <summary>
        /// Initializes a new instance of the <see cref="CrabValidator{T}"/> class.
        /// </summary>
        protected CrabValidator()
        {
            _descriptor = new ValidatorDescriptor(GetType(), typeof(T));
            _plan = new Lazy<ValidationPlan>(() => new ValidationPlanBuilder().Build(_descriptor));
        }

        /// <summary>
        /// Gets the captured validator descriptor.
        /// </summary>
        public ValidatorDescriptor Descriptor => _descriptor;

        /// <summary>
        /// Gets the validation plan for this validator.
        /// </summary>
        public ValidationPlan Plan => _plan.Value;

        /// <inheritdoc/>
        public ValidationResult Validate(T instance)
            => ValidationPlanExecutor.Execute(Plan, instance);

        /// <inheritdoc/>
        public ValidationResult Validate(ValidationContext<T> context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return ValidationPlanExecutor.Execute(Plan, context);
        }

        /// <inheritdoc/>
        public ValueTask<ValidationResult> ValidateAsync(T instance, CancellationToken cancellationToken = default)
            => ValidationPlanExecutor.ExecuteAsync(Plan, instance, cancellationToken);

        /// <inheritdoc/>
        public ValueTask<ValidationResult> ValidateAsync(ValidationContext<T> context, CancellationToken cancellationToken = default)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return ValidationPlanExecutor.ExecuteAsync(Plan, context, cancellationToken);
        }

        /// <summary>
        /// Captures a property rule.
        /// </summary>
        /// <typeparam name="TProperty">The property type.</typeparam>
        /// <param name="expression">The property expression.</param>
        /// <returns>A fluent property rule builder.</returns>
        protected ICrabRuleBuilder<T, TProperty> RuleFor<TProperty>(Expression<Func<T, TProperty>> expression)
        {
            var propertyRule = PropertyRuleDescriptor.Create(expression);
            _descriptor.AddRule(propertyRule);

            return new CrabRuleBuilder<T, TProperty>(propertyRule);
        }

        /// <summary>
        /// Captures nested validation for a child object property.
        /// </summary>
        /// <typeparam name="TProperty">The child model type.</typeparam>
        /// <param name="expression">The child property expression.</param>
        protected void ValidateNested<TProperty>(Expression<Func<T, TProperty>> expression)
        {
            var propertyRule = PropertyRuleDescriptor.Create(expression);
            propertyRule.SetNestedValidator(new NestedValidatorDescriptor(
                typeof(TProperty),
                typeof(CrabValidator<TProperty>),
                false,
                null));

            _descriptor.AddRule(propertyRule);
        }

        /// <summary>
        /// Captures nested validation for a child object property using an explicit validator type.
        /// </summary>
        /// <typeparam name="TValidator">The validator type.</typeparam>
        /// <param name="expression">The child property expression.</param>
        protected void ValidateNestedWith<TValidator>(Expression<Func<T, object>> expression)
            where TValidator : class
        {
            var modelType = GetValidatorModelType(typeof(TValidator));
            var propertyRule = PropertyRuleDescriptor.CreateUntyped(expression);
            if (!modelType.IsAssignableFrom(propertyRule.PropertyType))
            {
                throw new InvalidOperationException(
                    $"Validator for '{modelType.FullName}' cannot validate property '{propertyRule.PropertyPath}' of type '{propertyRule.PropertyType.FullName}'.");
            }

            propertyRule.SetNestedValidator(new NestedValidatorDescriptor(
                modelType,
                typeof(TValidator),
                false,
                null));

            _descriptor.AddRule(propertyRule);
        }

        /// <summary>
        /// Captures nested validation for each item in a collection property.
        /// </summary>
        /// <typeparam name="TElement">The collection item model type.</typeparam>
        /// <param name="expression">The collection property expression.</param>
        protected void ValidateEach<TElement>(Expression<Func<T, IEnumerable<TElement>>> expression)
        {
            var propertyRule = PropertyRuleDescriptor.Create(expression);
            propertyRule.SetNestedValidator(new NestedValidatorDescriptor(
                typeof(TElement),
                typeof(CrabValidator<TElement>),
                true,
                null));

            _descriptor.AddRule(propertyRule);
        }

        /// <summary>
        /// Captures nested validation for each item in a collection property using an explicit validator type.
        /// </summary>
        /// <typeparam name="TValidator">The validator type.</typeparam>
        /// <param name="expression">The collection property expression.</param>
        protected void ValidateEachWith<TValidator>(Expression<Func<T, object>> expression)
            where TValidator : class
        {
            var modelType = GetValidatorModelType(typeof(TValidator));
            var propertyRule = PropertyRuleDescriptor.CreateUntyped(expression);
            if (!CanEnumerateModelType(propertyRule.PropertyType, modelType))
            {
                throw new InvalidOperationException(
                    $"Validator for '{modelType.FullName}' cannot validate items from property '{propertyRule.PropertyPath}' of type '{propertyRule.PropertyType.FullName}'.");
            }

            propertyRule.SetNestedValidator(new NestedValidatorDescriptor(
                modelType,
                typeof(TValidator),
                true,
                null));

            _descriptor.AddRule(propertyRule);
        }

        private static bool CanEnumerateModelType(Type propertyType, Type modelType)
        {
            if (propertyType == typeof(string) || !typeof(System.Collections.IEnumerable).IsAssignableFrom(propertyType))
            {
                return false;
            }

            if (propertyType.IsArray)
            {
                return modelType.IsAssignableFrom(propertyType.GetElementType());
            }

            var elementTypes = propertyType
                .GetInterfaces()
                .Concat(new[] { propertyType })
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .Select(x => x.GetGenericArguments()[0])
                .ToArray();

            return elementTypes.Length == 0 || elementTypes.Any(modelType.IsAssignableFrom);
        }

        private static Type GetValidatorModelType(Type validatorType)
        {
            var current = validatorType;
            while (current != null && current != typeof(object))
            {
                if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(CrabValidator<>))
                {
                    return current.GetGenericArguments()[0];
                }

                current = current.BaseType;
            }

            throw new ArgumentException($"Type '{validatorType.FullName}' must derive from CrabValidator<T>.", nameof(validatorType));
        }
    }
}
