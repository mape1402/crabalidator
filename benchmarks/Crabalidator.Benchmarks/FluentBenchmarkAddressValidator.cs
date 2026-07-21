using FluentValidation;

namespace Crabalidator.Benchmarks
{
    public sealed class FluentBenchmarkAddressValidator : AbstractValidator<BenchmarkAddress>
    {
        public FluentBenchmarkAddressValidator()
        {
            RuleFor(x => x.PostalCode)
                .Length(5, 5);

            RuleFor(x => x.CountryCode)
                .Equal("MX");
        }
    }
}
