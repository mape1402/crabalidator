namespace Crabalidator.Planning
{
    /// <summary>
    /// Describes the planned validation work for a property.
    /// </summary>
    public sealed class PropertyValidationPlan
    {
        private readonly Func<object, object> _getValue;
        private readonly Func<object, bool> _shouldValidate;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyValidationPlan"/> class.
        /// </summary>
        /// <param name="order">The property rule order.</param>
        /// <param name="propertyName">The terminal property name.</param>
        /// <param name="propertyPath">The full property path.</param>
        /// <param name="propertyType">The property type.</param>
        /// <param name="cascadeMode">The cascade mode.</param>
        /// <param name="hasCondition">A value indicating whether the property has a configured condition.</param>
        /// <param name="rules">The rule plans.</param>
        /// <param name="nestedValidation">The nested validation plan.</param>
        /// <param name="getValue">The configured value accessor.</param>
        /// <param name="shouldValidate">The configured property condition.</param>
        internal PropertyValidationPlan(
            int order,
            string propertyName,
            string propertyPath,
            Type propertyType,
            CascadeMode cascadeMode,
            bool hasCondition,
            IReadOnlyList<RulePlan> rules,
            NestedValidationPlan nestedValidation,
            Func<object, object> getValue,
            Func<object, bool> shouldValidate)
        {
            Order = order;
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            PropertyPath = propertyPath ?? throw new ArgumentNullException(nameof(propertyPath));
            PropertyType = propertyType ?? throw new ArgumentNullException(nameof(propertyType));
            CascadeMode = cascadeMode;
            HasCondition = hasCondition;
            Rules = rules ?? throw new ArgumentNullException(nameof(rules));
            NestedValidation = nestedValidation;
            _getValue = getValue ?? throw new ArgumentNullException(nameof(getValue));
            _shouldValidate = shouldValidate ?? throw new ArgumentNullException(nameof(shouldValidate));
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
        /// Gets the cascade mode.
        /// </summary>
        public CascadeMode CascadeMode { get; }

        /// <summary>
        /// Gets a value indicating whether the property has a configured condition.
        /// </summary>
        public bool HasCondition { get; }

        /// <summary>
        /// Gets the planned rules.
        /// </summary>
        public IReadOnlyList<RulePlan> Rules { get; }

        /// <summary>
        /// Gets the nested validation plan, when configured.
        /// </summary>
        public NestedValidationPlan NestedValidation { get; }

        internal object GetValue(object instance)
            => _getValue(instance);

        internal bool ShouldValidate(object instance)
            => _shouldValidate(instance);
    }
}
