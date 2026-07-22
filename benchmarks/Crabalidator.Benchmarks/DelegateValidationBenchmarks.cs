using BenchmarkDotNet.Attributes;
using Crabalidator.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Crabalidator.Benchmarks
{
    [MemoryDiagnoser]
    public class DelegateValidationBenchmarks
    {
        private DelegateMustIntRequest _validMustInt;
        private DelegateMustIntRequest _invalidMustInt;
        private DelegateMustStringRequest _validMustString;
        private DelegateMustStringRequest _invalidMustString;
        private DelegateConditionRequest _validCondition;
        private DelegateConditionRequest _skippedCondition;
        private IValidator<DelegateMustIntRequest> _compiledMustInt;
        private IValidator<DelegateMustStringRequest> _compiledMustString;
        private IValidator<DelegateConditionRequest> _compiledCondition;
        private FluentDelegateMustIntValidator _fluentMustInt;
        private FluentDelegateMustStringValidator _fluentMustString;
        private FluentDelegateConditionValidator _fluentCondition;

        [GlobalSetup]
        public void Setup()
        {
            _validMustInt = new DelegateMustIntRequest { Age = 21 };
            _invalidMustInt = new DelegateMustIntRequest { Age = 15 };
            _validMustString = new DelegateMustStringRequest { Status = "ACTIVE" };
            _invalidMustString = new DelegateMustStringRequest { Status = "INACTIVE" };
            _validCondition = new DelegateConditionRequest
            {
                Age = 21,
                ValidateAge = true,
                Status = "ACTIVE",
                IsDraft = false
            };
            _skippedCondition = new DelegateConditionRequest
            {
                Age = 10,
                ValidateAge = false,
                Status = null,
                IsDraft = true
            };

            var services = new ServiceCollection();
            services.AddCrabalidator(registration =>
            {
                registration.AddValidator<DelegateMustIntValidator>();
                registration.AddValidator<DelegateMustStringValidator>();
                registration.AddValidator<DelegateConditionValidator>();
            });

            var provider = services.BuildServiceProvider();
            _compiledMustInt = provider.GetRequiredService<IValidator<DelegateMustIntRequest>>();
            _compiledMustString = provider.GetRequiredService<IValidator<DelegateMustStringRequest>>();
            _compiledCondition = provider.GetRequiredService<IValidator<DelegateConditionRequest>>();
            _fluentMustInt = new FluentDelegateMustIntValidator();
            _fluentMustString = new FluentDelegateMustStringValidator();
            _fluentCondition = new FluentDelegateConditionValidator();
        }

        [Benchmark(Baseline = true)]
        public ValidationResult Crabalidator_MustInt_Valid()
            => _compiledMustInt.Validate(_validMustInt);

        [Benchmark]
        public ValidationResult Crabalidator_MustInt_Invalid()
            => _compiledMustInt.Validate(_invalidMustInt);

        [Benchmark]
        public ValidationResult Crabalidator_MustString_Valid()
            => _compiledMustString.Validate(_validMustString);

        [Benchmark]
        public ValidationResult Crabalidator_MustString_Invalid()
            => _compiledMustString.Validate(_invalidMustString);

        [Benchmark]
        public ValidationResult Crabalidator_Conditions_Valid()
            => _compiledCondition.Validate(_validCondition);

        [Benchmark]
        public ValidationResult Crabalidator_Conditions_Skipped()
            => _compiledCondition.Validate(_skippedCondition);

        [Benchmark]
        public FluentValidation.Results.ValidationResult FluentValidation_MustInt_Valid()
            => _fluentMustInt.Validate(_validMustInt);

        [Benchmark]
        public FluentValidation.Results.ValidationResult FluentValidation_MustString_Valid()
            => _fluentMustString.Validate(_validMustString);

        [Benchmark]
        public FluentValidation.Results.ValidationResult FluentValidation_Conditions_Skipped()
            => _fluentCondition.Validate(_skippedCondition);
    }
}
