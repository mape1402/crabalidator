using Crabalidator.DependencyInjection;
using Crabalidator.Diagnostics;
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

        [Fact]
        public void Diagnostics_Describe_Registered_Validators()
        {
            var services = new ServiceCollection();
            services.AddCrabalidator(registration => registration.AddValidator<DiCustomerValidator>());

            var provider = services.BuildServiceProvider();
            var diagnostics = provider.GetRequiredService<ICrabalidatorDiagnostics>();

            var validators = diagnostics.GetRegisteredValidators();
            var description = diagnostics.DescribeRegisteredValidators();

            var validator = Assert.Single(validators);
            Assert.Equal(typeof(DiCustomerValidator), validator.ValidatorType);
            Assert.Equal(typeof(DiCustomer), validator.ModelType);
            Assert.Contains(typeof(DiCustomerValidator).FullName, description);
            Assert.Contains(typeof(DiCustomer).FullName, description);
        }

        [Fact]
        public void Diagnostics_Describe_Registered_Model_Plan()
        {
            var services = new ServiceCollection();
            services.AddCrabalidator(registration => registration.AddValidator<DiCustomerValidator>());

            var provider = services.BuildServiceProvider();
            var diagnostics = provider.GetRequiredService<ICrabalidatorDiagnostics>();

            var description = diagnostics.DescribePlan<DiCustomer>();

            Assert.Contains("Validator:", description);
            Assert.Contains("Model:", description);
            Assert.Contains("- #0 Name (String)", description);
            Assert.Contains("- #0 NotEmpty", description);
        }

        [Fact]
        public void Nested_Validator_Type_Is_Resolved_From_DependencyInjection()
        {
            var dependency = new DiValidationDependency("blocked");
            var services = new ServiceCollection();
            services.AddSingleton(dependency);
            services.AddCrabalidator(registration =>
            {
                registration.AddValidator<DiOrderValidator>();
                registration.AddValidator<DiAddressValidator>();
                registration.AddValidator<DiOrderItemValidator>();
            });

            var provider = services.BuildServiceProvider();
            var validator = provider.GetRequiredService<IValidator<DiOrder>>();

            var result = validator.Validate(new DiOrder
            {
                Address = new DiAddress { City = "blocked" },
                Items = new List<DiOrderItem>
                {
                    new DiOrderItem { Sku = "ok" },
                    new DiOrderItem { Sku = "blocked" }
                }
            });

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, x => x.PropertyPath == "Address.City");
            Assert.Contains(result.Errors, x => x.PropertyPath == "Items[1].Sku");
            Assert.True(dependency.CallCount >= 2);
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

        public sealed class DiOrderValidator : CrabValidator<DiOrder>
        {
            public DiOrderValidator()
            {
                ValidateNested(x => x.Address);

                ValidateEach(x => x.Items);
            }
        }

        public sealed class DiAddressValidator : CrabValidator<DiAddress>
        {
            private readonly DiValidationDependency _dependency;

            public DiAddressValidator(DiValidationDependency dependency)
            {
                _dependency = dependency;

                RuleFor(x => x.City)
                    .Must(_dependency.IsAllowed);
            }
        }

        public sealed class DiOrderItemValidator : CrabValidator<DiOrderItem>
        {
            private readonly DiValidationDependency _dependency;

            public DiOrderItemValidator(DiValidationDependency dependency)
            {
                _dependency = dependency;

                RuleFor(x => x.Sku)
                    .Must(_dependency.IsAllowed);
            }
        }

        public sealed class DiValidationDependency
        {
            private readonly string _blockedValue;

            public DiValidationDependency(string blockedValue)
            {
                _blockedValue = blockedValue;
            }

            public int CallCount { get; private set; }

            public bool IsAllowed(string value)
            {
                CallCount++;
                return value != _blockedValue;
            }
        }

        public sealed class DiOrder
        {
            public DiAddress Address { get; set; }

            public List<DiOrderItem> Items { get; set; }
        }

        public sealed class DiAddress
        {
            public string City { get; set; }
        }

        public sealed class DiOrderItem
        {
            public string Sku { get; set; }
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
