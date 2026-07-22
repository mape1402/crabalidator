namespace Crabalidator.Samples.Basic
{
    public sealed class CustomerValidator : CrabValidator<Customer>
    {
        public CustomerValidator()
        {
            RuleFor(x => x.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Customer name is required.")
                .WithErrorCode("CUSTOMER_NAME_REQUIRED")
                .MaximumLength(20);

            RuleFor(x => x.Age)
                .GreaterThanOrEqualTo(18);

            RuleFor(x => x.Email)
                .NotNull()
                .MaximumLength(25);

            ValidateNested(x => x.Address);

            RuleFor(x => x.Status)
                .Must(x => x == "ACTIVE")
                .WithMessage("Customer status must be ACTIVE.")
                .Unless(x => x.IsDraft);

            RuleFor(x => x.Tier)
                .Equal("PRO");

            RuleFor(x => x.ReferralCode)
                .NotEqual("BLOCKED");

            RuleFor(x => x.RiskScore)
                .LessThanOrEqualTo(50);
        }
    }
}
