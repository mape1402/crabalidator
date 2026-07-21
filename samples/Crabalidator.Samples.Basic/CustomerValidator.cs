namespace Crabalidator.Samples.Basic
{
    public sealed class CustomerValidator : CrabValidator<Customer>
    {
        public CustomerValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(20);

            RuleFor(x => x.Age)
                .GreaterThanOrEqualTo(18);

            RuleFor(x => x.Email)
                .NotNull()
                .MaximumLength(25);

            RuleFor(x => x.Address.PostalCode)
                .Length(5, 5);
        }
    }
}
