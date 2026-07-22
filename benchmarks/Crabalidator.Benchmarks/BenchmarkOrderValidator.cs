namespace Crabalidator.Benchmarks
{
    public sealed class BenchmarkOrderValidator : CrabValidator<BenchmarkOrder>
    {
        public BenchmarkOrderValidator()
        {
            ValidateNested(x => x.Customer);

            RuleFor(x => x.Items)
                .NotEmpty();

            ValidateEach(x => x.Items);
        }
    }
}
