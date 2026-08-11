using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Crabalidator.Testing
{
    /// <summary>
    /// Builds lightweight Crabalidator test hosts.
    /// </summary>
    public sealed class CrabalidatorTestHostBuilder
    {
        private readonly List<Assembly> _assemblies = new List<Assembly>();
        private readonly List<Action<IServiceCollection>> _configureServices = new List<Action<IServiceCollection>>();

        /// <summary>
        /// Adds validators discovered from an assembly.
        /// </summary>
        /// <param name="assembly">The assembly that contains validators.</param>
        /// <returns>The same builder.</returns>
        public CrabalidatorTestHostBuilder UseValidatorsFromAssembly(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            if (!_assemblies.Contains(assembly))
            {
                _assemblies.Add(assembly);
            }

            return this;
        }

        /// <summary>
        /// Adds validators discovered from assemblies.
        /// </summary>
        /// <param name="assemblies">The assemblies that contain validators.</param>
        /// <returns>The same builder.</returns>
        public CrabalidatorTestHostBuilder UseValidatorsFromAssemblies(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies ?? Array.Empty<Assembly>())
            {
                UseValidatorsFromAssembly(assembly);
            }

            return this;
        }

        /// <summary>
        /// Configures services before the host is built.
        /// </summary>
        /// <param name="configure">The service configuration callback.</param>
        /// <returns>The same builder.</returns>
        public CrabalidatorTestHostBuilder ConfigureServices(Action<IServiceCollection> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            _configureServices.Add(configure);
            return this;
        }

        /// <summary>
        /// Builds the test host.
        /// </summary>
        /// <returns>The test host.</returns>
        public CrabalidatorTestHost Build()
        {
            var services = new ServiceCollection();
            services.AddCrabalidatorTesting(_assemblies.ToArray());

            for (var i = 0; i < _configureServices.Count; i++)
            {
                _configureServices[i](services);
            }

            return new CrabalidatorTestHost(services.BuildServiceProvider());
        }

        /// <summary>
        /// Builds the test host asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The test host.</returns>
        public ValueTask<CrabalidatorTestHost> BuildAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new ValueTask<CrabalidatorTestHost>(Build());
        }
    }
}
