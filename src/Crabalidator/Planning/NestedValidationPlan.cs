using System.Collections;

namespace Crabalidator.Planning
{
    /// <summary>
    /// Describes nested validation attached to a property.
    /// </summary>
    public sealed class NestedValidationPlan
    {
        private readonly Func<object, ValidationResult> _validate;

        internal NestedValidationPlan(Type modelType, bool isCollection, Func<object, ValidationResult> validate)
        {
            ModelType = modelType ?? throw new ArgumentNullException(nameof(modelType));
            IsCollection = isCollection;
            _validate = validate ?? throw new ArgumentNullException(nameof(validate));
        }

        /// <summary>
        /// Gets the nested model type.
        /// </summary>
        public Type ModelType { get; }

        /// <summary>
        /// Gets a value indicating whether this plan validates collection items.
        /// </summary>
        public bool IsCollection { get; }

        internal ValidationResult Validate(object instance)
            => _validate(instance);

        internal IEnumerable Enumerate(object value)
            => value as IEnumerable ?? Array.Empty<object>();
    }
}
