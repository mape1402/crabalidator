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
        public void TestValidate_Validates_Direct_Validators_With_Typed_Assertions()
        {
            var result = new CreateHeroRequestValidator()
                .TestValidate(new CreateHeroRequest("", 101, CId.Empty));

            result.ShouldHaveErrorFor(request => request.Alias);
            result.ShouldHaveErrorFor(request => request.PowerLevel)
                .WithErrorCode("hero.powerLevel.range");
            result.ShouldHaveErrorFor(request => request.TeamId)
                .WithMessage("Team is required.");
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
        public async Task TestValidateAsync_Validates_Direct_Async_Validators()
        {
            var result = await new AsyncCustomerRequestValidator()
                .TestValidateAsync(new AsyncCustomerRequest { Status = "PENDING" });

            result.ShouldHaveErrorFor(request => request.Status)
                .WithErrorCode("customer.status.active");
        }

        [Fact]
        public async Task CrabalidatorTestHost_Resolves_Validators_From_Di()
        {
            await using var host = await CrabalidatorTestHost
                .Create()
                .UseValidatorsFromAssembly(typeof(CreateCustomerRequestValidator).Assembly)
                .BuildAsync();

            var result = await host.ValidateAsync(new CreateCustomerRequest());

            result.ShouldHaveErrorFor(request => request.Email)
                .WithErrorCode("customer.email.required");
        }

        [Fact]
        public void CrabalidatorTestHost_Supports_Sync_Validation()
        {
            using var host = CrabalidatorTestHost
                .Create()
                .UseValidatorsFromAssembly(typeof(CreateHeroRequestValidator).Assembly)
                .Build();

            var result = host.Validate(new CreateHeroRequest("", 101, CId.Empty));

            result.ShouldHaveErrorFor(request => request.Alias);
            result.ShouldHaveErrorFor(request => request.PowerLevel);
            result.ShouldHaveErrorFor(request => request.TeamId);
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

    public readonly record struct CId(string Value)
    {
        public static CId Empty => new CId(string.Empty);
    }

    public sealed record CreateHeroRequest(string Alias, int PowerLevel, CId TeamId);

    public sealed class CreateHeroRequestValidator : CrabValidator<CreateHeroRequest>
    {
        public CreateHeroRequestValidator()
        {
            RuleFor(x => x.Alias)
                .NotEmpty()
                .WithMessage("Alias is required.")
                .WithErrorCode("hero.alias.required");

            RuleFor(x => x.PowerLevel)
                .InclusiveBetween(1, 100)
                .WithMessage("Power level must be between 1 and 100.")
                .WithErrorCode("hero.powerLevel.range");

            RuleFor(x => x.TeamId)
                .Must(value => !string.IsNullOrWhiteSpace(value.Value))
                .WithMessage("Team is required.")
                .WithErrorCode("hero.team.required");
        }
    }
}
