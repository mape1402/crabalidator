using BenchmarkDotNet.Attributes;
using Crabalidator.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Crabalidator.Benchmarks
{
    [MemoryDiagnoser]
    public class DirectMustBenchmarks
    {
        private PublicDirectMustRequest _validRequest;
        private PublicDirectMustRequest _invalidRequest;
        private IValidator<PublicDirectMustRequest> _staticValidator;
        private IValidator<PublicDirectMustRequest> _instanceValidator;
        private IValidator<PublicDirectMustRequest> _lambdaFallbackValidator;
        private FluentPublicDirectMustValidator _fluentValidator;

        [GlobalSetup]
        public void Setup()
        {
            _validRequest = new PublicDirectMustRequest { Code = "CR-123" };
            _invalidRequest = new PublicDirectMustRequest { Code = "NO-123" };

            _staticValidator = CreateValidator<PublicStaticMustValidator>();
            _instanceValidator = CreateValidator<PublicInstanceMustValidator>();
            _lambdaFallbackValidator = CreateValidator<LambdaFallbackMustValidator>();
            _fluentValidator = new FluentPublicDirectMustValidator();
        }

        [Benchmark(Baseline = true)]
        public ValidationResult Crabalidator_StaticMust_Valid()
            => _staticValidator.Validate(_validRequest);

        [Benchmark]
        public ValidationResult Crabalidator_StaticMust_Invalid()
            => _staticValidator.Validate(_invalidRequest);

        [Benchmark]
        public ValidationResult Crabalidator_InstanceMust_Valid()
            => _instanceValidator.Validate(_validRequest);

        [Benchmark]
        public ValidationResult Crabalidator_InstanceMust_Invalid()
            => _instanceValidator.Validate(_invalidRequest);

        [Benchmark]
        public ValidationResult Crabalidator_LambdaFallbackMust_Valid()
            => _lambdaFallbackValidator.Validate(_validRequest);

        [Benchmark]
        public ValidationResult Crabalidator_LambdaFallbackMust_Invalid()
            => _lambdaFallbackValidator.Validate(_invalidRequest);

        [Benchmark]
        public FluentValidation.Results.ValidationResult FluentValidation_StaticMust_Valid()
            => _fluentValidator.Validate(_validRequest);

        [Benchmark]
        public FluentValidation.Results.ValidationResult FluentValidation_StaticMust_Invalid()
            => _fluentValidator.Validate(_invalidRequest);

        private static IValidator<PublicDirectMustRequest> CreateValidator<TValidator>()
            where TValidator : CrabValidator<PublicDirectMustRequest>
        {
            var services = new ServiceCollection();
            services.AddCrabalidator(registration => registration.AddValidator<TValidator>());

            return services
                .BuildServiceProvider()
                .GetRequiredService<IValidator<PublicDirectMustRequest>>();
        }
    }
}
