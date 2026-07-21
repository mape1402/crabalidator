namespace Crabalidator.Benchmarks
{
    public sealed class BenchmarkOrderValidator : CrabValidator<BenchmarkOrder>
    {
        public BenchmarkOrderValidator()
        {
            RuleFor(x => x.Customer)
                .SetValidator(new BenchmarkCustomerValidator());

            RuleFor(x => x.Items)
                .NotEmpty()
                .RuleForEach(new BenchmarkOrderItemValidator());
        }
    }
}
