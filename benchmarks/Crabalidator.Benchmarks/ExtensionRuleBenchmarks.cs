using BenchmarkDotNet.Attributes;
using Crabalidator.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Crabalidator.Benchmarks
{
    [MemoryDiagnoser]
    public class ExtensionRuleBenchmarks
    {
        private ExtensionRuleRequest _validRequest;
        private ExtensionRuleRequest _invalidRequest;
        private IValidator<ExtensionRuleRequest> _compiledValidator;
        private FluentExtensionRuleValidator _fluentValidator;

        [GlobalSetup]
        public void Setup()
        {
            _validRequest = new ExtensionRuleRequest
            {
                Name = "CRABVALID",
                Email = "crab@example.com",
                Age = 37,
                Score = 88,
                Items = new List<string> { "one", "two", "three" },
                Status = "ACTIVE",
                Optional = null
            };

            _invalidRequest = new ExtensionRuleRequest
            {
                Name = "x",
                Email = "not-email",
                Age = 0,
                Score = 101,
                Items = new List<string> { "one" },
                Status = "BLOCKED",
                Optional = "not-null"
            };

            var services = new ServiceCollection();
            services.AddCrabalidator(registration => registration.AddValidator<ExtensionRuleValidator>());

            var provider = services.BuildServiceProvider();
            _compiledValidator = provider.GetRequiredService<IValidator<ExtensionRuleRequest>>();
            _fluentValidator = new FluentExtensionRuleValidator();
        }

        [Benchmark(Baseline = true)]
        public ValidationResult Crabalidator_Extensions_Valid()
            => _compiledValidator.Validate(_validRequest);

        [Benchmark]
        public ValidationResult Crabalidator_Extensions_Invalid()
            => _compiledValidator.Validate(_invalidRequest);

        [Benchmark]
        public FluentValidation.Results.ValidationResult FluentValidation_Extensions_Valid()
            => _fluentValidator.Validate(_validRequest);

        [Benchmark]
        public FluentValidation.Results.ValidationResult FluentValidation_Extensions_Invalid()
            => _fluentValidator.Validate(_invalidRequest);
    }
}
