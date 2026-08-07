using System.Reflection;

namespace Crabalidator.Testing
{
    /// <summary>
    /// Describes assemblies registered for Crabalidator testing.
    /// </summary>
    public sealed class CrabalidatorTestingAssemblyCatalog
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CrabalidatorTestingAssemblyCatalog"/> class.
        /// </summary>
        /// <param name="assemblies">The registered assemblies.</param>
        public CrabalidatorTestingAssemblyCatalog(IEnumerable<Assembly> assemblies)
        {
            Assemblies = (assemblies ?? Array.Empty<Assembly>())
                .Where(x => x != null)
                .Distinct()
                .ToArray();
        }

        /// <summary>
        /// Gets registered assemblies.
        /// </summary>
        public IReadOnlyList<Assembly> Assemblies { get; }
    }
}
