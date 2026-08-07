using Crabalidator.Configuration;
using Crabalidator.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Crabalidator.Testing.Tests
{
    public class CrabalidatorTestingTests
    {
        [Fact]
        public async Task AddCrabalidatorTesting_Registers_Validators_From_Assembly()
        {
            var services = new ServiceCollection();

            services.AddCrabalidatorTesting(typeof(CreateCustomerRequestValidator).Assembly);

            var provider = services.BuildServiceProvider();
            var validator = provider.GetRequiredService<IAsyncValidator<CreateCustomerRequest>>();

            var result = await validator.ValidateAsync(new CreateCustomerRequest());

            result.ShouldHaveErrorFor<CreateCustomerRequest>(x => x.Email)
                .WithErrorCount(2)
                .WithMessage("Email is required.")
                .WithErrorCode("customer.email.required")
                .WithSeverity(ValidationSeverity.Error);
        }

        [Fact]
        public async Task AddCrabalidatorTestingAdapter_Registers_Validators_And_Catalog()
        {
            var services = new ServiceCollection();

            services.AddCrabalidatorTestingAdapter(typeof(CreateCustomerRequestValidator).Assembly);

            var provider = services.BuildServiceProvider();
            var catalog = provider.GetRequiredService<CrabalidatorTestingAssemblyCatalog>();
            var validator = provider.GetRequiredService<IAsyncValidator<CreateCustomerRequest>>();

            var result = await validator.ValidateAsync(new CreateCustomerRequest { Email = "ada@example.com" });

            Assert.Contains(typeof(CreateCustomerRequestValidator).Assembly, catalog.Assemblies);
            result.ShouldBeValid();
        }

        [Fact]
        public async Task CrabalidatorTest_For_Validates_A_Specific_Validator_Directly()
        {
            var result = await CrabalidatorTest
                .For<CreateCustomerRequestValidator>()
                .ValidateAsync(new CreateCustomerRequest());

            result.ShouldHaveErrorFor(x => x.Email)
                .WithErrorCount(2);
            result.ShouldHaveErrorMessage("Email is required.");
            result.ShouldHaveErrorCode("customer.email.required");
        }

        [Fact]
        public async Task CrabalidatorTest_For_Supports_Async_Rules()
        {
            var result = await CrabalidatorTest
                .For<AsyncCustomerRequestValidator>()
                .ValidateAsync(new AsyncCustomerRequest { Status = "PENDING" });

            result.ShouldHaveErrorFor(x => x.Status)
                .WithMessage("Status must be ACTIVE.")
                .WithErrorCode("customer.status.active");
        }

        [Fact]
        public void ShouldBeValid_Throws_When_Result_Has_Errors()
        {
            var result = ValidationResult.FromFailures(new[]
            {
                new ValidationFailure("Email", "Email", "Email is required.")
            });

            var exception = Assert.Throws<CrabalidatorAssertionException>(() => result.ShouldBeValid());

            Assert.Contains("Expected validation to succeed", exception.Message);
        }

        [Fact]
        public void ShouldHaveErrorFor_Supports_Nested_Property_Paths()
        {
            var result = ValidationResult.FromFailures(new[]
            {
                new ValidationFailure("Email", "Customer.Email", "Email is required.")
            });

            result.ShouldHaveErrorFor<CreateOrderRequest>(x => x.Customer.Email)
                .WithMessage("Email is required.");
        }
    }

    public sealed class CreateCustomerRequestValidator : CrabValidator<CreateCustomerRequest>
    {
        public CreateCustomerRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required.")
                .WithErrorCode("customer.email.required");

            RuleFor(x => x.Email)
                .Must(x => !string.IsNullOrWhiteSpace(x) && x.Contains('@'))
                .WithMessage("Email must be valid.")
                .WithErrorCode("customer.email.invalid")
                .WithSeverity(ValidationSeverity.Warning);
        }
    }

    public sealed class AsyncCustomerRequestValidator : CrabValidator<AsyncCustomerRequest>
    {
        public AsyncCustomerRequestValidator()
        {
            RuleFor(x => x.Status)
                .MustAsync(async (value, cancellationToken) =>
                {
                    await Task.Yield();
                    cancellationToken.ThrowIfCancellationRequested();
                    return value == "ACTIVE";
                })
                .WithMessage("Status must be ACTIVE.")
                .WithErrorCode("customer.status.active");
        }
    }

    public sealed class CreateCustomerRequest
    {
        public string Email { get; set; }
    }

    public sealed class AsyncCustomerRequest
    {
        public string Status { get; set; }
    }

    public sealed class CreateOrderRequest
    {
        public CreateCustomerRequest Customer { get; set; }
    }
}
