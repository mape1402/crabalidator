using Crabalidator.Configuration;

namespace Crabalidator.Tests
{
    public class RuleExtensionMethodTests
    {
        [Fact]
        public void Extension_Methods_Preserve_Known_Rule_Metadata()
        {
            var validator = new ExtensionCustomerValidator();

            var nameRules = validator.Plan.Properties.Single(x => x.PropertyPath == nameof(ExtensionCustomer.Name)).Rules;
            var ageRules = validator.Plan.Properties.Single(x => x.PropertyPath == nameof(ExtensionCustomer.Age)).Rules;
            var scoreRules = validator.Plan.Properties.Single(x => x.PropertyPath == nameof(ExtensionCustomer.Score)).Rules;
            var itemRules = validator.Plan.Properties.Single(x => x.PropertyPath == nameof(ExtensionCustomer.Items)).Rules;

            Assert.Equal(
                new[]
                {
                    RuleKind.NotEmpty,
                    RuleKind.MinimumLength,
                    RuleKind.MaximumLength,
                    RuleKind.Must,
                    RuleKind.Must,
                    RuleKind.Must,
                    RuleKind.Must
                },
                nameRules.Select(x => x.Kind));

            Assert.Equal(new[] { RuleKind.NotEmpty, RuleKind.Must }, ageRules.Select(x => x.Kind));
            Assert.Equal(new[] { RuleKind.GreaterThan, RuleKind.LessThanOrEqualTo }, scoreRules.Select(x => x.Kind));
            Assert.Equal(new[] { RuleKind.NotEmpty, RuleKind.Must, RuleKind.Must }, itemRules.Select(x => x.Kind));
        }

        [Fact]
        public void Extension_Methods_Validate_By_Data_Type()
        {
            var validator = new ExtensionCustomerValidator();

            var result = validator.Validate(new ExtensionCustomer
            {
                Name = "x",
                Email = "bad-email",
                Age = 0,
                Score = 101,
                Items = new List<string> { "one" },
                Status = "X",
                Optional = "not-null"
            });

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, x => x.PropertyPath == nameof(ExtensionCustomer.Name) && x.ErrorMessage.Contains("at least 3"));
            Assert.Contains(result.Errors, x => x.PropertyPath == nameof(ExtensionCustomer.Name) && x.ErrorMessage.Contains("correct format"));
            Assert.Contains(result.Errors, x => x.PropertyPath == nameof(ExtensionCustomer.Email) && x.ErrorMessage.Contains("valid email"));
            Assert.Contains(result.Errors, x => x.PropertyPath == nameof(ExtensionCustomer.Age) && x.ErrorMessage.Contains("empty"));
            Assert.Contains(result.Errors, x => x.PropertyPath == nameof(ExtensionCustomer.Score) && x.ErrorMessage.Contains("less than or equal"));
            Assert.Contains(result.Errors, x => x.PropertyPath == nameof(ExtensionCustomer.Items) && x.ErrorMessage.Contains("at least 2"));
            Assert.Contains(result.Errors, x => x.PropertyPath == nameof(ExtensionCustomer.Status) && x.ErrorMessage.Contains("allowed"));
            Assert.Contains(result.Errors, x => x.PropertyPath == nameof(ExtensionCustomer.Optional) && x.ErrorMessage.Contains("null"));
        }

        private sealed class ExtensionCustomerValidator : CrabValidator<ExtensionCustomer>
        {
            public ExtensionCustomerValidator()
            {
                RuleFor(x => x.Name)
                    .NotEmpty()
                    .MinimumLength(3)
                    .MaximumLength(8)
                    .Matches("^CRAB")
                    .StartsWith("CR")
                    .EndsWith("AB")
                    .Contains("RA");

                RuleFor(x => x.Email)
                    .EmailAddress();

                RuleFor(x => x.Age)
                    .NotEmpty()
                    .InclusiveBetween(1, 10);

                RuleFor(x => x.Score)
                    .GreaterThan(0)
                    .LessThanOrEqualTo(100);

                RuleFor(x => x.Items)
                    .NotEmpty()
                    .MinimumCount(2)
                    .MaximumCount(3);

                RuleFor(x => x.Status)
                    .In("A", "B")
                    .NotIn("X");

                RuleFor(x => x.Optional)
                    .Null();
            }
        }

        private sealed class ExtensionCustomer
        {
            public string Name { get; set; }

            public string Email { get; set; }

            public int Age { get; set; }

            public int Score { get; set; }

            public List<string> Items { get; set; }

            public string Status { get; set; }

            public string Optional { get; set; }
        }
    }
}
