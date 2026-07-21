using Crabalidator.DependencyInjection;
using Crabalidator.Generation.Dynabee;
using Microsoft.Extensions.DependencyInjection;

namespace Crabalidator.Tests
{
    public class AsyncValidationTests
    {
        [Fact]
        public async Task ValidateAsync_Executes_Async_Custom_Rule()
        {
            var validator = new AsyncCustomerValidator();

            var result = await validator.ValidateAsync(new AsyncCustomer { Status = "PENDING" });

            Assert.False(result.IsValid);
            var failure = Assert.Single(result.Errors);
            Assert.Equal("Status", failure.PropertyPath);
            Assert.Equal("Status must be ACTIVE.", failure.ErrorMessage);
        }

        [Fact]
        public void Validate_Throws_When_Plan_Requires_Async()
        {
            var validator = new AsyncCustomerValidator();

            var exception = Assert.Throws<InvalidOperationException>(() => validator.Validate(new AsyncCustomer()));

            Assert.Contains("ValidateAsync", exception.Message);
        }

        [Fact]
        public async Task ValidateAsync_Observes_Cancellation()
        {
            var validator = new AsyncCustomerValidator();
            using var source = new CancellationTokenSource();
            source.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await validator.ValidateAsync(new AsyncCustomer { Status = "ACTIVE" }, source.Token));
        }

        [Fact]
        public async Task ValidateAsync_Executes_Async_Nested_Validator()
        {
            var validator = new AsyncOrderValidator();

            var result = await validator.ValidateAsync(new AsyncOrder
            {
                Customer = new AsyncCustomer { Status = "PENDING" }
            });

            Assert.False(result.IsValid);
            var failure = Assert.Single(result.Errors);
            Assert.Equal("Customer.Status", failure.PropertyPath);
        }

        [Fact]
        public async Task Crabalidator_Service_Executes_Async_Validator()
        {
            var services = new ServiceCollection();
            services.AddCrabalidator(registration => registration.AddValidator<AsyncCustomerValidator>());

            var provider = services.BuildServiceProvider();
            var crabalidator = provider.GetRequiredService<ICrabalidator>();

            var result = await crabalidator.ValidateAsync(new AsyncCustomer { Status = "PENDING" });

            Assert.False(result.IsValid);
            Assert.Equal("Status", Assert.Single(result.Errors).PropertyPath);
        }

        [Fact]
        public void Compiled_Adapter_Throws_On_Sync_Execution_For_Async_Validator()
        {
            var services = new ServiceCollection();
            services.AddCrabalidator(registration => registration.AddValidator<AsyncCustomerValidator>());

            var provider = services.BuildServiceProvider();
            var validator = provider.GetRequiredService<IValidator<AsyncCustomer>>();

            var exception = Assert.Throws<InvalidOperationException>(() => validator.Validate(new AsyncCustomer()));

            Assert.Contains("ValidateAsync", exception.Message);
        }

        [Fact]
        public void Dynabee_Backend_Rejects_Async_Plans()
        {
            var validator = new AsyncCustomerValidator();
            var backend = new DynabeeValidationGenerationBackend();

            Assert.True(validator.Plan.RequiresAsync);
            Assert.False(backend.Supports(validator.Plan));
            Assert.Throws<NotSupportedException>(() => backend.Compile(validator.Plan));
        }

        private sealed class AsyncCustomerValidator : CrabValidator<AsyncCustomer>
        {
            public AsyncCustomerValidator()
            {
                RuleFor(x => x.Status)
                    .MustAsync(async (value, cancellationToken) =>
                    {
                        await Task.Yield();
                        cancellationToken.ThrowIfCancellationRequested();
                        return value == "ACTIVE";
                    })
                    .WithMessage("Status must be ACTIVE.");
            }
        }

        private sealed class AsyncOrderValidator : CrabValidator<AsyncOrder>
        {
            public AsyncOrderValidator()
            {
                RuleFor(x => x.Customer)
                    .SetValidator(new AsyncCustomerValidator());
            }
        }

        private sealed class AsyncCustomer
        {
            public string Status { get; set; }
        }

        private sealed class AsyncOrder
        {
            public AsyncCustomer Customer { get; set; }
        }
    }
}
