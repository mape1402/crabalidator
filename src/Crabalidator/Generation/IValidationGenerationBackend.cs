using Crabalidator.Planning;

namespace Crabalidator.Generation
{
    /// <summary>
    /// Compiles validation plans into executable validator implementations.
    /// </summary>
    public interface IValidationGenerationBackend
    {
        /// <summary>
        /// Gets the backend name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Determines whether this backend supports the specified plan.
        /// </summary>
        /// <param name="plan">The validation plan.</param>
        /// <returns>True when supported; otherwise, false.</returns>
        bool Supports(ValidationPlan plan);

        /// <summary>
        /// Compiles the specified validation plan.
        /// </summary>
        /// <param name="plan">The validation plan.</param>
        /// <returns>The compiled validator.</returns>
        CompiledValidator Compile(ValidationPlan plan);
    }
}
