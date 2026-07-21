using FluentValidation;

namespace Crabalidator.Benchmarks
{
    public sealed class FluentBenchmarkCustomerValidator : AbstractValidator<BenchmarkCustomer>
    {
        public FluentBenchmarkCustomerValidator()
        {
            RuleFor(x => x.Name)
                .Cascade(FluentValidation.CascadeMode.Stop)
                .NotEmpty()
                .MaximumLength(20);

            RuleFor(x => x.Age)
                .GreaterThanOrEqualTo(18);

            RuleFor(x => x.Email)
                .NotNull()
                .MaximumLength(40);

            RuleFor(x => x.Address)
                .SetValidator(new FluentBenchmarkAddressValidator());

            RuleFor(x => x.Status)
                .Must(x => x == "ACTIVE");

            RuleFor(x => x.Tier)
                .Equal("PRO");

            RuleFor(x => x.ReferralCode)
                .NotEqual("BLOCKED");

            RuleFor(x => x.RiskScore)
                .LessThanOrEqualTo(50);
        }
    }
}
