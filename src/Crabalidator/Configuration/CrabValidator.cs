using System.Linq.Expressions;
using Crabalidator.Configuration;
using Crabalidator.Planning;

namespace Crabalidator
{
    /// <summary>
    /// Base class for configuring validators.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    public abstract class CrabValidator<T> : IValidator<T>, IAsyncValidator<T>
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
            => Validate(new ValidationContext<T>(instance));

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
            => ValidateAsync(new ValidationContext<T>(instance), cancellationToken);

        /// <inheritdoc/>
        public ValueTask<ValidationResult> ValidateAsync(ValidationContext<T> context, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new ValueTask<ValidationResult>(Validate(context));
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
    }
}
