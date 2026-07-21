namespace Crabalidator.Diagnostics
{
    /// <summary>
    /// Describes a validator registered with Crabalidator.
    /// </summary>
    public sealed class CrabalidatorValidatorDiagnostic
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CrabalidatorValidatorDiagnostic"/> class.
        /// </summary>
        /// <param name="validatorType">The validator type.</param>
        /// <param name="modelType">The validated model type.</param>
        public CrabalidatorValidatorDiagnostic(Type validatorType, Type modelType)
        {
            ValidatorType = validatorType ?? throw new ArgumentNullException(nameof(validatorType));
            ModelType = modelType ?? throw new ArgumentNullException(nameof(modelType));
        }

        /// <summary>
        /// Gets the validator type.
        /// </summary>
        public Type ValidatorType { get; }

        /// <summary>
        /// Gets the validated model type.
        /// </summary>
        public Type ModelType { get; }
    }
}
