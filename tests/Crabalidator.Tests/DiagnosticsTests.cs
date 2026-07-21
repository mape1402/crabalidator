using Crabalidator.Configuration;
using Crabalidator.Diagnostics;

namespace Crabalidator.Tests
{
    public class DiagnosticsTests
    {
        [Fact]
        public void DescribePlan_Includes_Plan_Metadata_And_Rules()
        {
            var validator = new DiagnosticCustomerValidator();

            var description = validator.DescribePlan();

            Assert.Contains("Requires async: True", description);
            Assert.Contains("Requires context: False", description);
            Assert.Contains("- #0 Name (String)", description);
            Assert.Contains("- #0 NotEmpty: Name is required.", description);
            Assert.Contains("Error code: NAME_REQUIRED", description);
            Assert.Contains("Severity: Warning", description);
            Assert.Contains("- #1 MaximumLength: 'Name' length must be no more than 10.", description);
            Assert.Contains("Maximum: 10", description);
            Assert.Contains("- #0 MustAsync async: 'Status' is not valid.", description);
            Assert.Contains("- SetValidator(DiagnosticAddress) async", description);
        }

        [Fact]
        public void DescribePlan_Includes_Diagnostics()
        {
            var validator = new EmptyDiagnosticValidator();

            var description = validator.DescribePlan();

            Assert.Contains("Diagnostics:", description);
            Assert.Contains("Warning CRABPLAN001", description);
            Assert.Contains("Validator does not contain any property rules.", description);
        }

        private sealed class DiagnosticCustomerValidator : CrabValidator<DiagnosticCustomer>
        {
            public DiagnosticCustomerValidator()
            {
                RuleFor(x => x.Name)
                    .NotEmpty()
                    .WithMessage("Name is required.")
                    .WithErrorCode("NAME_REQUIRED")
                    .WithSeverity(ValidationSeverity.Warning)
                    .MaximumLength(10);

                RuleFor(x => x.Status)
                    .MustAsync((value, _) => new ValueTask<bool>(value == "ACTIVE"));

                RuleFor(x => x.Address)
                    .SetValidator(new DiagnosticAddressValidator());
            }
        }

        private sealed class DiagnosticAddressValidator : CrabValidator<DiagnosticAddress>
        {
            public DiagnosticAddressValidator()
            {
                RuleFor(x => x.PostalCode)
                    .MustAsync((value, _) => new ValueTask<bool>(value == "12345"));
            }
        }

        private sealed class EmptyDiagnosticValidator : CrabValidator<DiagnosticCustomer>
        {
        }

        private sealed class DiagnosticCustomer
        {
            public string Name { get; set; }

            public string Status { get; set; }

            public DiagnosticAddress Address { get; set; }
        }

        private sealed class DiagnosticAddress
        {
            public string PostalCode { get; set; }
        }
    }
}
