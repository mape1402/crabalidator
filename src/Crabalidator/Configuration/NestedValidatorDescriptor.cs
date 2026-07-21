using System.Collections;

namespace Crabalidator.Configuration
{
    /// <summary>
    /// Describes a nested validator attached to a property.
    /// </summary>
    public sealed class NestedValidatorDescriptor
    {
        private readonly Func<object, ValidationResult> _validate;
        private readonly Func<object, CancellationToken, ValueTask<ValidationResult>> _validateAsync;

        internal NestedValidatorDescriptor(
            Type modelType,
            bool isCollection,
            bool isAsync,
            Func<object, ValidationResult> validate,
            Func<object, CancellationToken, ValueTask<ValidationResult>> validateAsync)
        {
            ModelType = modelType ?? throw new ArgumentNullException(nameof(modelType));
            IsCollection = isCollection;
            IsAsync = isAsync;
            _validate = validate ?? throw new ArgumentNullException(nameof(validate));
            _validateAsync = validateAsync ?? throw new ArgumentNullException(nameof(validateAsync));
        }

        /// <summary>
        /// Gets the nested model type.
        /// </summary>
        public Type ModelType { get; }

        /// <summary>
        /// Gets a value indicating whether this validator applies to collection items.
        /// </summary>
        public bool IsCollection { get; }

        /// <summary>
        /// Gets a value indicating whether this nested validator requires async execution.
        /// </summary>
        public bool IsAsync { get; }

        internal ValidationResult Validate(object instance)
            => _validate(instance);

        internal ValueTask<ValidationResult> ValidateAsync(object instance, CancellationToken cancellationToken)
            => _validateAsync(instance, cancellationToken);

        internal static bool IsEnumerableButNotString(Type type)
            => type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);
    }
}
