using Crabalidator.Diagnostics;

namespace Crabalidator.DependencyInjection
{
    /// <summary>
    /// Captures validator registrations for runtime diagnostics.
    /// </summary>
    internal sealed class CrabalidatorRegistrationCatalog
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CrabalidatorRegistrationCatalog"/> class.
        /// </summary>
        /// <param name="validators">The registered validators.</param>
        public CrabalidatorRegistrationCatalog(IReadOnlyList<CrabalidatorValidatorDiagnostic> validators)
        {
            Validators = validators ?? throw new ArgumentNullException(nameof(validators));
        }

        /// <summary>
        /// Gets the registered validators.
        /// </summary>
        public IReadOnlyList<CrabalidatorValidatorDiagnostic> Validators { get; }
    }
}
