using Crabalidator.DependencyInjection;
using Crabalidator.Runtime;
using Microsoft.Extensions.DependencyInjection;

namespace Crabalidator.Tests
{
    public class DependencyInjectionTests
    {
        [Fact]
        public void AddCrabalidator_Registers_Explicit_Validator()
        {
            var services = new ServiceCollection();
            services.AddCrabalidator(registration => registration.AddValidator<DiCustomerValidator>());

            var provider = services.BuildServiceProvider();
            var validator = provider.GetRequiredService<IValidator<DiCustomer>>();

            var result = validator.Validate(new DiCustomer { Name = "", Age = 12 });

            Assert.False(result.IsValid);
            Assert.Equal(new[] { "Name", "Age" }, result.Errors.Select(x => x.PropertyPath));
        }

        [Fact]
        public void AddCrabalidator_Scans_Public_Validators_From_Assembly()
        {
            var services = new ServiceCollection();
            services.AddCrabalidator(typeof(PublicAssemblyScanCustomerValidator).Assembly);

            var provider = services.BuildServiceProvider();
            var validator = provider.GetRequiredService<IValidator<AssemblyScanCustomer>>();

            var result = validator.Validate(new AssemblyScanCustomer { Code = "" });

            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Equal("Code", result.Errors[0].PropertyPath);
        }

        [Fact]
        public void Crabalidator_Service_Resolves_And_Executes_Typed_Validator()
        {
            var services = new ServiceCollection();
            services.AddCrabalidator(registration => registration.AddValidator<DiCustomerValidator>());

            var provider = services.BuildServiceProvider();
            var crabalidator = provider.GetRequiredService<ICrabalidator>();

            var result = crabalidator.Validate(new DiCustomer { Name = "Ada", Age = 37 });

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Registered_Validator_Uses_Compiled_Registry()
        {
            var services = new ServiceCollection();
            services.AddCrabalidator(registration => registration.AddValidator<DiCustomerValidator>());

            var provider = services.BuildServiceProvider();
            var configuredValidator = provider.GetRequiredService<CrabValidator<DiCustomer>>();
            var registry = provider.GetRequiredService<ICompiledValidatorRegistry>();

            var compiled = registry.GetOrAdd(configuredValidator);

            Assert.NotEqual(typeof(DiCustomerValidator), compiled.ValidatorType);
            Assert.Contains("DiCustomerValidator", compiled.ValidatorType.Name);
        }

        public sealed class DiCustomerValidator : CrabValidator<DiCustomer>
        {
            public DiCustomerValidator()
            {
                RuleFor(x => x.Name).NotEmpty();
                RuleFor(x => x.Age).GreaterThanOrEqualTo(18);
            }
        }

        public sealed class DiCustomer
        {
            public string Name { get; set; }

            public int Age { get; set; }
        }
    }

    public sealed class PublicAssemblyScanCustomerValidator : CrabValidator<AssemblyScanCustomer>
    {
        public PublicAssemblyScanCustomerValidator()
        {
            RuleFor(x => x.Code).NotEmpty();
        }
    }

    public sealed class AssemblyScanCustomer
    {
        public string Code { get; set; }
    }
}
