namespace Crabalidator.Diagnostics
{
    /// <summary>
    /// Provides runtime diagnostics for registered Crabalidator validators.
    /// </summary>
    public interface ICrabalidatorDiagnostics
    {
        /// <summary>
        /// Gets validators registered through Crabalidator.
        /// </summary>
        /// <returns>The registered validators.</returns>
        IReadOnlyList<CrabalidatorValidatorDiagnostic> GetRegisteredValidators();

        /// <summary>
        /// Describes validators registered through Crabalidator.
        /// </summary>
        /// <returns>A readable validator registration description.</returns>
        string DescribeRegisteredValidators();

        /// <summary>
        /// Describes the validation plan for the registered validator of a model type.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <returns>A readable plan description.</returns>
        string DescribePlan<T>();
    }
}
