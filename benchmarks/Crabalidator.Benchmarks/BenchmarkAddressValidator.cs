namespace Crabalidator.Benchmarks
{
    public sealed class BenchmarkAddressValidator : CrabValidator<BenchmarkAddress>
    {
        public BenchmarkAddressValidator()
        {
            RuleFor(x => x.PostalCode)
                .Length(5, 5);

            RuleFor(x => x.CountryCode)
                .Equal("MX");
        }
    }
}
