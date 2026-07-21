using System.Collections;

namespace Crabalidator.Planning
{
    /// <summary>
    /// Describes nested validation attached to a property.
    /// </summary>
    public sealed class NestedValidationPlan
    {
        private readonly Func<object, ValidationResult> _validate;
        private readonly Func<object, CancellationToken, ValueTask<ValidationResult>> _validateAsync;

        internal NestedValidationPlan(
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
        /// Gets a value indicating whether this plan validates collection items.
        /// </summary>
        public bool IsCollection { get; }

        /// <summary>
        /// Gets a value indicating whether this nested validation requires async execution.
        /// </summary>
        public bool IsAsync { get; }

        internal ValidationResult Validate(object instance)
            => _validate(instance);

        internal ValueTask<ValidationResult> ValidateAsync(object instance, CancellationToken cancellationToken)
            => _validateAsync(instance, cancellationToken);

        internal IEnumerable Enumerate(object value)
            => value as IEnumerable ?? Array.Empty<object>();
    }
}
