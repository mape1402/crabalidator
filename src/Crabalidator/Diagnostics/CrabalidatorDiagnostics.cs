using System.Text;
using Crabalidator.DependencyInjection;
using Crabalidator.Planning;
using Microsoft.Extensions.DependencyInjection;

namespace Crabalidator.Diagnostics
{
    /// <summary>
    /// Default runtime diagnostics implementation.
    /// </summary>
    internal sealed class CrabalidatorDiagnostics : ICrabalidatorDiagnostics
    {
        private readonly IServiceProvider _services;
        private readonly CrabalidatorRegistrationCatalog _catalog;

        public CrabalidatorDiagnostics(
            IServiceProvider services,
            CrabalidatorRegistrationCatalog catalog)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }

        public IReadOnlyList<CrabalidatorValidatorDiagnostic> GetRegisteredValidators()
            => _catalog.Validators;

        public string DescribeRegisteredValidators()
        {
            var builder = new StringBuilder();
            builder.AppendLine("Registered validators:");

            foreach (var validator in _catalog.Validators)
            {
                builder.AppendLine($"- {validator.ModelType.FullName}: {validator.ValidatorType.FullName}");
            }

            return builder.ToString();
        }

        public string DescribePlan<T>()
        {
            var validator = _services.GetRequiredService<CrabValidator<T>>();
            var plan = new ValidationPlanBuilder(_services).Build(validator.Descriptor);
            return plan.DescribePlan();
        }
    }
}
