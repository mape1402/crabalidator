namespace Crabalidator.Benchmarks
{
    public sealed class BenchmarkCustomerValidator : CrabValidator<BenchmarkCustomer>
    {
        public BenchmarkCustomerValidator()
        {
            RuleFor(x => x.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .MaximumLength(20);

            RuleFor(x => x.Age)
                .GreaterThanOrEqualTo(18);

            RuleFor(x => x.Email)
                .NotNull()
                .MaximumLength(40);

            RuleFor(x => x.Address)
                .SetValidator(new BenchmarkAddressValidator());

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
