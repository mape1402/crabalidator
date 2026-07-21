using Crabalidator.Configuration;

namespace Crabalidator.Tests
{
    public class FluentRuleSurfaceTests
    {
        [Fact]
        public void WithMessage_ErrorCode_And_Severity_Are_Applied_To_Failure()
        {
            var validator = new FluentCustomerValidator();

            var result = validator.Validate(new FluentCustomer
            {
                Name = "",
                Age = 20,
                Status = "ACTIVE"
            });

            var failure = Assert.Single(result.Errors);
            Assert.Equal("Name is required.", failure.ErrorMessage);
            Assert.Equal("NAME_REQUIRED", failure.ErrorCode);
            Assert.Equal(ValidationSeverity.Warning, failure.Severity);
        }

        [Fact]
        public void When_Skips_Property_Rules_When_Condition_Is_False()
        {
            var validator = new FluentCustomerValidator();

            var result = validator.Validate(new FluentCustomer
            {
                Name = "Ada",
                Age = 17,
                ValidateAge = false,
                Status = "ACTIVE"
            });

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Unless_Skips_Property_Rules_When_Condition_Is_True()
        {
            var validator = new FluentCustomerValidator();

            var result = validator.Validate(new FluentCustomer
            {
                Name = "Ada",
                Age = 20,
                IsDraft = true,
                Status = "BAD"
            });

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Cascade_Stop_Stops_After_First_Property_Failure()
        {
            var validator = new CascadeCustomerValidator();

            var result = validator.Validate(new FluentCustomer { Name = "" });

            Assert.Single(result.Errors);
            Assert.Equal("Name is required.", result.Errors[0].ErrorMessage);
        }

        [Fact]
        public void Must_Uses_Custom_Predicate()
        {
            var validator = new FluentCustomerValidator();

            var result = validator.Validate(new FluentCustomer
            {
                Name = "Ada",
                Age = 20,
                Status = "BAD"
            });

            var failure = Assert.Single(result.Errors);
            Assert.Equal("Status must be ACTIVE.", failure.ErrorMessage);
            Assert.Equal("Status", failure.PropertyPath);
        }

        [Fact]
        public void Plan_Carries_Fluent_Surface_Metadata()
        {
            var validator = new CascadeCustomerValidator();

            var property = Assert.Single(validator.Plan.Properties);
            Assert.Equal(CascadeMode.Stop, property.CascadeMode);
            Assert.Equal(new[] { RuleKind.NotEmpty, RuleKind.MaximumLength }, property.Rules.Select(x => x.Kind));
            Assert.Equal("Name is required.", property.Rules[0].Failure.ErrorMessage);
            Assert.Equal("NAME_REQUIRED", property.Rules[0].Failure.ErrorCode);
            Assert.Equal(ValidationSeverity.Warning, property.Rules[0].Failure.Severity);
        }

        private sealed class FluentCustomerValidator : CrabValidator<FluentCustomer>
        {
            public FluentCustomerValidator()
            {
                RuleFor(x => x.Name)
                    .NotEmpty()
                    .WithMessage("Name is required.")
                    .WithErrorCode("NAME_REQUIRED")
                    .WithSeverity(ValidationSeverity.Warning);

                RuleFor(x => x.Age)
                    .GreaterThanOrEqualTo(18)
                    .When(x => x.ValidateAge);

                RuleFor(x => x.Status)
                    .Must(x => x == "ACTIVE")
                    .WithMessage("Status must be ACTIVE.")
                    .Unless(x => x.IsDraft);
            }
        }

        private sealed class CascadeCustomerValidator : CrabValidator<FluentCustomer>
        {
            public CascadeCustomerValidator()
            {
                RuleFor(x => x.Name)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                    .WithMessage("Name is required.")
                    .WithErrorCode("NAME_REQUIRED")
                    .WithSeverity(ValidationSeverity.Warning)
                    .MaximumLength(2);
            }
        }

        private sealed class FluentCustomer
        {
            public string Name { get; set; }

            public int Age { get; set; }

            public bool ValidateAge { get; set; } = true;

            public bool IsDraft { get; set; }

            public string Status { get; set; }
        }
    }
}
