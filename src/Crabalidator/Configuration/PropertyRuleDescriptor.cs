using System.Linq.Expressions;

namespace Crabalidator.Configuration
{
    /// <summary>
    /// Describes validation rules attached to a property.
    /// </summary>
    public sealed class PropertyRuleDescriptor
    {
        private readonly List<RuleDescriptor> _rules = new List<RuleDescriptor>();
        private readonly Func<object, object> _accessor;
        private Func<object, bool> _condition;

        internal PropertyRuleDescriptor(
            string propertyName,
            string propertyPath,
            Type propertyType,
            Func<object, object> accessor)
        {
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            PropertyPath = propertyPath ?? throw new ArgumentNullException(nameof(propertyPath));
            PropertyType = propertyType ?? throw new ArgumentNullException(nameof(propertyType));
            _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
        }

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
        /// Gets the configured rules.
        /// </summary>
        public IReadOnlyList<RuleDescriptor> Rules => _rules;

        /// <summary>
        /// Gets the cascade mode.
        /// </summary>
        public CascadeMode CascadeMode { get; private set; }

        /// <summary>
        /// Gets the nested validator, when configured.
        /// </summary>
        public NestedValidatorDescriptor NestedValidator { get; private set; }

        internal bool HasCondition => _condition != null;

        internal static PropertyRuleDescriptor Create<T, TProperty>(Expression<Func<T, TProperty>> expression)
        {
            var path = PropertyPathResolver.Resolve(expression);
            var accessor = expression.Compile();

            return new PropertyRuleDescriptor(
                path.PropertyName,
                path.PropertyPathValue,
                typeof(TProperty),
                instance => accessor((T)instance));
        }

        internal void AddRule(RuleDescriptor rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }

            _rules.Add(rule);
        }

        internal void SetCascadeMode(CascadeMode cascadeMode)
            => CascadeMode = cascadeMode;

        internal void SetCondition(Func<object, bool> condition)
            => _condition = condition ?? throw new ArgumentNullException(nameof(condition));

        internal void SetNestedValidator(NestedValidatorDescriptor nestedValidator)
            => NestedValidator = nestedValidator ?? throw new ArgumentNullException(nameof(nestedValidator));

        internal bool ShouldValidate(object instance)
            => _condition == null || _condition(instance);

        internal object GetValue(object instance)
        {
            if (instance == null)
            {
                return null;
            }

            try
            {
                return _accessor(instance);
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }
    }
}
