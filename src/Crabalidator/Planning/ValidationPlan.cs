namespace Crabalidator.Planning
{
    /// <summary>
    /// Describes backend-neutral validation work for a validator.
    /// </summary>
    public sealed class ValidationPlan
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationPlan"/> class.
        /// </summary>
        /// <param name="validatorType">The validator type.</param>
        /// <param name="modelType">The model type.</param>
        /// <param name="properties">The property validation plans.</param>
        /// <param name="diagnostics">The plan diagnostics.</param>
        public ValidationPlan(
            Type validatorType,
            Type modelType,
            IReadOnlyList<PropertyValidationPlan> properties,
            IReadOnlyList<ValidationPlanDiagnostic> diagnostics)
        {
            ValidatorType = validatorType ?? throw new ArgumentNullException(nameof(validatorType));
            ModelType = modelType ?? throw new ArgumentNullException(nameof(modelType));
            Properties = properties ?? throw new ArgumentNullException(nameof(properties));
            Diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
            RequiresAsync = Properties.SelectMany(x => x.Rules).Any(x => x.IsAsync);
            RequiresContext = Properties.SelectMany(x => x.Rules).Any(x => x.RequiresContext);
        }

        /// <summary>
        /// Gets the validator type.
        /// </summary>
        public Type ValidatorType { get; }

        /// <summary>
        /// Gets the model type.
        /// </summary>
        public Type ModelType { get; }

        /// <summary>
        /// Gets the property validation plans.
        /// </summary>
        public IReadOnlyList<PropertyValidationPlan> Properties { get; }

        /// <summary>
        /// Gets the plan diagnostics.
        /// </summary>
        public IReadOnlyList<ValidationPlanDiagnostic> Diagnostics { get; }

        /// <summary>
        /// Gets a value indicating whether the plan requires async execution.
        /// </summary>
        public bool RequiresAsync { get; }

        /// <summary>
        /// Gets a value indicating whether the plan requires validation context.
        /// </summary>
        public bool RequiresContext { get; }
    }
}
