namespace Crabalidator.Samples.Basic
{
    public sealed class AddressValidator : CrabValidator<Address>
    {
        public AddressValidator()
        {
            RuleFor(x => x.PostalCode)
                .Length(5, 5);

            RuleFor(x => x.CountryCode)
                .Equal("MX");

            RuleFor(x => x.State)
                .NotEmpty()
                .When(x => x.CountryCode == "MX");
        }
    }
}
