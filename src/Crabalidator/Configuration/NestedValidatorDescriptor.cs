using System.Collections;
using Crabalidator.Planning;

namespace Crabalidator.Configuration
{
    /// <summary>
    /// Describes a nested validator attached to a property.
    /// </summary>
    public sealed class NestedValidatorDescriptor
    {
        internal NestedValidatorDescriptor(
            Type modelType,
            Type validatorType,
            bool isCollection,
            ICrabValidatorDescriptorSource validator)
        {
            ModelType = modelType ?? throw new ArgumentNullException(nameof(modelType));
            ValidatorType = validatorType ?? throw new ArgumentNullException(nameof(validatorType));
            IsCollection = isCollection;
            Validator = validator;
        }

        /// <summary>
        /// Gets the nested model type.
        /// </summary>
        public Type ModelType { get; }

        /// <summary>
        /// Gets the nested validator type.
        /// </summary>
        public Type ValidatorType { get; }

        /// <summary>
        /// Gets a value indicating whether this validator applies to collection items.
        /// </summary>
        public bool IsCollection { get; }

        /// <summary>
        /// Gets a value indicating whether this nested validator requires async execution.
        /// </summary>
        internal ICrabValidatorDescriptorSource Validator { get; }

        internal static bool IsEnumerableButNotString(Type type)
            => type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);
    }
}
