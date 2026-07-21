using System.Collections;

namespace Crabalidator.Configuration
{
    /// <summary>
    /// Describes a nested validator attached to a property.
    /// </summary>
    public sealed class NestedValidatorDescriptor
    {
        private readonly Func<object, ValidationResult> _validate;

        internal NestedValidatorDescriptor(Type modelType, bool isCollection, Func<object, ValidationResult> validate)
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
        /// Gets a value indicating whether this validator applies to collection items.
        /// </summary>
        public bool IsCollection { get; }

        internal ValidationResult Validate(object instance)
            => _validate(instance);

        internal static bool IsEnumerableButNotString(Type type)
            => type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);
    }
}
