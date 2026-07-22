using BenchmarkDotNet.Attributes;
using Crabalidator.DependencyInjection;
using Crabalidator.Planning;
using Microsoft.Extensions.DependencyInjection;

namespace Crabalidator.Benchmarks
{
    [MemoryDiagnoser]
    public class ValidationBenchmarks
    {
        private BenchmarkCustomer _validCustomer;
        private BenchmarkCustomer _invalidCustomer;
        private BenchmarkOrder _invalidOrder;
        private BenchmarkRegistrationRequest _invalidRegistration;
        private BenchmarkCustomerValidator _configuredCustomerValidator;
        private ValidationPlan _customerPlan;
        private IValidator<BenchmarkCustomer> _compiledCustomerValidator;
        private IValidator<BenchmarkOrder> _compiledOrderValidator;
        private IAsyncValidator<BenchmarkRegistrationRequest> _asyncRegistrationValidator;
        private FluentBenchmarkCustomerValidator _fluentCustomerValidator;
        private FluentBenchmarkOrderValidator _fluentOrderValidator;
        private FluentBenchmarkRegistrationRequestValidator _fluentRegistrationValidator;

        [GlobalSetup]
        public void Setup()
        {
            _validCustomer = BenchmarkCustomerFactory.CreateValid();
            _invalidCustomer = BenchmarkCustomerFactory.CreateInvalid();
            _invalidOrder = BenchmarkOrderFactory.CreateInvalid();
            _invalidRegistration = new BenchmarkRegistrationRequest
            {
                Username = "taken",
                Email = "candidate@example.com"
            };

            _configuredCustomerValidator = new BenchmarkCustomerValidator();
            _customerPlan = _configuredCustomerValidator.Plan;

            var services = new ServiceCollection();
            services.AddCrabalidator(registration =>
            {
                registration.AddValidator<BenchmarkCustomerValidator>();
                registration.AddValidator<BenchmarkAddressValidator>();
                registration.AddValidator<BenchmarkOrderValidator>();
                registration.AddValidator<BenchmarkOrderItemValidator>();
                registration.AddValidator<BenchmarkRegistrationRequestValidator>();
            });

            var provider = services.BuildServiceProvider();
            _compiledCustomerValidator = provider.GetRequiredService<IValidator<BenchmarkCustomer>>();
            _compiledOrderValidator = provider.GetRequiredService<IValidator<BenchmarkOrder>>();
            _asyncRegistrationValidator = provider.GetRequiredService<IAsyncValidator<BenchmarkRegistrationRequest>>();

            _fluentCustomerValidator = new FluentBenchmarkCustomerValidator();
            _fluentOrderValidator = new FluentBenchmarkOrderValidator();
            _fluentRegistrationValidator = new FluentBenchmarkRegistrationRequestValidator();
        }

        [Benchmark(Baseline = true)]
        public ValidationResult DirectValidator_Valid()
            => _configuredCustomerValidator.Validate(_validCustomer);

        [Benchmark]
        public ValidationResult InterpretedPlan_Valid()
            => _customerPlan.Execute(_validCustomer);

        [Benchmark]
        public ValidationResult CompiledDiValidator_Valid()
            => _compiledCustomerValidator.Validate(_validCustomer);

        [Benchmark]
        public ValidationResult CompiledDiValidator_Invalid()
            => _compiledCustomerValidator.Validate(_invalidCustomer);

        [Benchmark]
        public ValidationResult CompiledNestedValidator_Invalid()
            => _compiledOrderValidator.Validate(_invalidOrder);

        [Benchmark]
        public ValueTask<ValidationResult> AsyncValidator_Invalid()
            => _asyncRegistrationValidator.ValidateAsync(_invalidRegistration);

        [Benchmark]
        public FluentValidation.Results.ValidationResult FluentValidation_Valid()
            => _fluentCustomerValidator.Validate(_validCustomer);

        [Benchmark]
        public FluentValidation.Results.ValidationResult FluentValidation_Invalid()
            => _fluentCustomerValidator.Validate(_invalidCustomer);

        [Benchmark]
        public FluentValidation.Results.ValidationResult FluentValidation_NestedInvalid()
            => _fluentOrderValidator.Validate(_invalidOrder);

        [Benchmark]
        public Task<FluentValidation.Results.ValidationResult> FluentValidation_AsyncInvalid()
            => _fluentRegistrationValidator.ValidateAsync(_invalidRegistration);
    }
}
