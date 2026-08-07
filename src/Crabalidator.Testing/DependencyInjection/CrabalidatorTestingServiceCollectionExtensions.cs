using System.Reflection;
using Crabalidator.DependencyInjection;
using Crabalidator.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Service registration helpers for validator tests.
    /// </summary>
    public static class CrabalidatorTestingServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Crabalidator testing services and validators from assemblies.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="assemblies">The assemblies that contain validators.</param>
        /// <returns>The same service collection.</returns>
        public static IServiceCollection AddCrabalidatorTesting(
            this IServiceCollection services,
            params Assembly[] assemblies)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddCrabalidator(assemblies ?? Array.Empty<Assembly>());
            services.AddSingleton(new CrabalidatorTestingAssemblyCatalog(assemblies ?? Array.Empty<Assembly>()));
            return services;
        }

        /// <summary>
        /// Adds adapter-friendly Crabalidator testing services for external test hosts.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="assemblies">The assemblies that contain validators.</param>
        /// <returns>The same service collection.</returns>
        public static IServiceCollection AddCrabalidatorTestingAdapter(
            this IServiceCollection services,
            params Assembly[] assemblies)
            => services.AddCrabalidatorTesting(assemblies);
    }
}
