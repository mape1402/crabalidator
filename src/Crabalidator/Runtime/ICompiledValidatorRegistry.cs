using Crabalidator.Generation;

namespace Crabalidator.Runtime
{
    /// <summary>
    /// Provides compiled validators.
    /// </summary>
    public interface ICompiledValidatorRegistry
    {
        /// <summary>
        /// Gets or compiles a validator.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="validator">The configured validator.</param>
        /// <returns>The compiled validator.</returns>
        CompiledValidator GetOrAdd<T>(CrabValidator<T> validator);
    }
}
