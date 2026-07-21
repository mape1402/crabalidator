namespace Crabalidator.Planning
{
    /// <summary>
    /// Describes the planned validation work for a property.
    /// </summary>
    public sealed class PropertyValidationPlan
    {
        private readonly Func<object, object> _getValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyValidationPlan"/> class.
        /// </summary>
        /// <param name="order">The property rule order.</param>
        /// <param name="propertyName">The terminal property name.</param>
        /// <param name="propertyPath">The full property path.</param>
        /// <param name="propertyType">The property type.</param>
        /// <param name="rules">The rule plans.</param>
        /// <param name="getValue">The configured value accessor.</param>
        internal PropertyValidationPlan(
            int order,
            string propertyName,
            string propertyPath,
            Type propertyType,
            IReadOnlyList<RulePlan> rules,
            Func<object, object> getValue)
        {
            Order = order;
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            PropertyPath = propertyPath ?? throw new ArgumentNullException(nameof(propertyPath));
            PropertyType = propertyType ?? throw new ArgumentNullException(nameof(propertyType));
            Rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _getValue = getValue ?? throw new ArgumentNullException(nameof(getValue));
        }

        /// <summary>
        /// Gets the property rule order.
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// Gets the terminal property name.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Gets the full property path.
        /// </summary>
        public string PropertyPath { get; }

        /// <summary>
        /// Gets the property type.
        /// </summary>
        public Type PropertyType { get; }

        /// <summary>
        /// Gets the planned rules.
        /// </summary>
        public IReadOnlyList<RulePlan> Rules { get; }

        internal object GetValue(object instance)
            => _getValue(instance);
    }
}
