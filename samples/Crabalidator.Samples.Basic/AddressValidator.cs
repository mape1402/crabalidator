namespace Crabalidator.Samples.Basic
{
    public sealed class AddressValidator : CrabValidator<Address>
    {
        public AddressValidator()
        {
            RuleFor(x => x.PostalCode)
                .Length(5, 5);
        }
    }
}
