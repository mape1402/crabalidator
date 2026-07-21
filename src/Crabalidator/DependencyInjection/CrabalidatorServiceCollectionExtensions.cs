using System.Reflection;
using Crabalidator.Diagnostics;
using Crabalidator.Generation;
using Crabalidator.Generation.Dynabee;
using Crabalidator.Planning;
using Crabalidator.Runtime;
using DynaBee.FluentApi.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Crabalidator.DependencyInjection
{
    /// <summary>
    /// Service registration helpers for Crabalidator.
    /// </summary>
    public static class CrabalidatorServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Crabalidator services and validators from assemblies.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="assemblies">The assemblies to scan.</param>
        /// <returns>The same service collection.</returns>
        public static IServiceCollection AddCrabalidator(this IServiceCollection services, params Assembly[] assemblies)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return services.AddCrabalidator(registration =>
            {
                if (assemblies == null)
                {
                    return;
                }

                foreach (var assembly in assemblies.Where(x => x != null))
                {
                    registration.AddValidators(assembly);
                }
            });
        }

        /// <summary>
        /// Adds Crabalidator services.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configure">The registration callback.</param>
        /// <returns>The same service collection.</returns>
        public static IServiceCollection AddCrabalidator(
            this IServiceCollection services,
            Action<CrabalidatorRegistrationBuilder> configure)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var registration = new CrabalidatorRegistrationBuilder();
            configure?.Invoke(registration);

            services.AddSingleton<IDynaBeeAssemblyBuilderFactory, DynaBeeAssemblyBuilderFactory>();
            services.AddSingleton<IValidationPlanBuilder, ValidationPlanBuilder>();
            services.AddSingleton<IValidationGenerationBackend, DynabeeValidationGenerationBackend>();
            services.AddSingleton<ICompiledValidatorRegistry, CompiledValidatorRegistry>();
            services.AddSingleton(CreateRegistrationCatalog(registration));
            services.AddTransient<ICrabalidatorDiagnostics, CrabalidatorDiagnostics>();
            services.AddTransient<ICrabalidator, Runtime.Crabalidator>();

            foreach (var validatorType in registration.ValidatorTypes)
            {
                RegisterValidator(services, validatorType);
            }

            return services;
        }

        private static CrabalidatorRegistrationCatalog CreateRegistrationCatalog(CrabalidatorRegistrationBuilder registration)
        {
            var validators = registration.ValidatorTypes
                .Select(x =>
                {
                    CrabalidatorRegistrationBuilder.TryGetModelType(x, out var modelType);
                    return new CrabalidatorValidatorDiagnostic(x, modelType);
                })
                .ToArray();

            return new CrabalidatorRegistrationCatalog(validators);
        }

        private static void RegisterValidator(IServiceCollection services, Type validatorType)
        {
            if (!CrabalidatorRegistrationBuilder.TryGetModelType(validatorType, out var modelType))
            {
                throw new ArgumentException($"Type '{validatorType.FullName}' must derive from CrabValidator<T>.", nameof(validatorType));
            }

            var crabValidatorType = typeof(CrabValidator<>).MakeGenericType(modelType);
            var validatorServiceType = typeof(IValidator<>).MakeGenericType(modelType);
            var asyncValidatorServiceType = typeof(IAsyncValidator<>).MakeGenericType(modelType);
            var adapterType = typeof(CompiledValidatorAdapter<>).MakeGenericType(modelType);

            services.AddTransient(validatorType);
            services.AddTransient(crabValidatorType, validatorType);
            services.AddTransient(validatorServiceType, adapterType);
            services.AddTransient(asyncValidatorServiceType, adapterType);
        }
    }
}
