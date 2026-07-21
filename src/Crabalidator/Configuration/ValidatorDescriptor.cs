namespace Crabalidator.Configuration
{
    /// <summary>
    /// Describes a configured validator.
    /// </summary>
    public sealed class ValidatorDescriptor
    {
        private readonly List<PropertyRuleDescriptor> _rules = new List<PropertyRuleDescriptor>();

        internal ValidatorDescriptor(Type validatorType, Type modelType)
        {
            ValidatorType = validatorType ?? throw new ArgumentNullException(nameof(validatorType));
            ModelType = modelType ?? throw new ArgumentNullException(nameof(modelType));
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
        /// Gets the configured property rules.
        /// </summary>
        public IReadOnlyList<PropertyRuleDescriptor> Rules => _rules;

        internal void AddRule(PropertyRuleDescriptor rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }

            _rules.Add(rule);
        }
    }
}
