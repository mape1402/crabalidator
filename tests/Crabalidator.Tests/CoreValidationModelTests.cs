using Crabalidator.Configuration;
using Crabalidator.Diagnostics;

namespace Crabalidator.Tests
{
    public class CoreValidationModelTests
    {
        [Fact]
        public void Validate_Returns_Success_When_All_Rules_Pass()
        {
            var validator = new CustomerValidator();

            var result = validator.Validate(new Customer
            {
                Name = "Ada",
                Age = 37,
                Email = "ada@example.com",
                Address = new Address { PostalCode = "12345" }
            });

            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Validate_Returns_Failures_When_Rules_Fail()
        {
            var validator = new CustomerValidator();

            var result = validator.Validate(new Customer
            {
                Name = " ",
                Age = 17,
                Email = "averylongemail@example.com",
                Address = new Address { PostalCode = "123" }
            });

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, x => x.PropertyName == nameof(Customer.Name) && x.PropertyPath == nameof(Customer.Name));
            Assert.Contains(result.Errors, x => x.PropertyName == nameof(Customer.Age) && x.PropertyPath == nameof(Customer.Age));
            Assert.Contains(result.Errors, x => x.PropertyName == nameof(Customer.Email) && x.PropertyPath == nameof(Customer.Email));
            Assert.Contains(result.Errors, x => x.PropertyName == nameof(Address.PostalCode) && x.PropertyPath == "Address.PostalCode");
        }

        [Fact]
        public void Validate_Treats_Null_Intermediate_Property_As_Null_Value()
        {
            var validator = new CustomerValidator();

            var result = validator.Validate(new Customer
            {
                Name = "Ada",
                Age = 37,
                Email = "ada@example.com",
                Address = null
            });

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Descriptor_Captures_Property_Rules_And_Rule_Kinds()
        {
            var validator = new CustomerValidator();

            Assert.Equal(typeof(CustomerValidator), validator.Descriptor.ValidatorType);
            Assert.Equal(typeof(Customer), validator.Descriptor.ModelType);
            Assert.Equal(4, validator.Descriptor.Rules.Count);

            var nameRule = validator.Descriptor.Rules.Single(x => x.PropertyPath == nameof(Customer.Name));
            Assert.Equal(typeof(string), nameRule.PropertyType);
            Assert.Equal(new[] { RuleKind.NotEmpty, RuleKind.MaximumLength }, nameRule.Rules.Select(x => x.Kind));

            var ageRule = validator.Descriptor.Rules.Single(x => x.PropertyPath == nameof(Customer.Age));
            Assert.Equal(RuleKind.GreaterThanOrEqualTo, ageRule.Rules.Single().Kind);
            Assert.Equal(18, ageRule.Rules.Single().ComparisonValue);
        }

        [Fact]
        public void DescribeConfiguration_Returns_Readable_Diagnostics()
        {
            var validator = new CustomerValidator();

            var description = validator.Descriptor.DescribeConfiguration();

            Assert.Contains("Validator:", description);
            Assert.Contains("Model:", description);
            Assert.Contains("- Name (String)", description);
            Assert.Contains("  - NotEmpty", description);
            Assert.Contains("- Address.PostalCode (String)", description);
        }

        [Fact]
        public async Task ValidateAsync_Returns_Sync_Result_For_Sync_Rules()
        {
            var validator = new CustomerValidator();

            var result = await validator.ValidateAsync(new Customer());

            Assert.False(result.IsValid);
        }

        [Fact]
        public void RuleFor_Rejects_Non_Member_Expressions()
        {
            var exception = Assert.Throws<ArgumentException>(() => new InvalidExpressionValidator());

            Assert.Contains("direct member access", exception.Message);
        }

        private sealed class CustomerValidator : CrabValidator<Customer>
        {
            public CustomerValidator()
            {
                RuleFor(x => x.Name)
                    .NotEmpty()
                    .MaximumLength(10);

                RuleFor(x => x.Age)
                    .GreaterThanOrEqualTo(18);

                RuleFor(x => x.Email)
                    .NotNull()
                    .MaximumLength(20);

                RuleFor(x => x.Address.PostalCode)
                    .Length(5, 5);
            }
        }

        private sealed class InvalidExpressionValidator : CrabValidator<Customer>
        {
            public InvalidExpressionValidator()
            {
                RuleFor(x => x.Name.ToUpperInvariant()).NotEmpty();
            }
        }

        private sealed class Customer
        {
            public string Name { get; set; }

            public int Age { get; set; }

            public string Email { get; set; }

            public Address Address { get; set; } = new Address();
        }

        private sealed class Address
        {
            public string PostalCode { get; set; }
        }
    }
}
