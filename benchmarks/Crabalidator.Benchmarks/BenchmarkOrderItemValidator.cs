namespace Crabalidator.Benchmarks
{
    public sealed class BenchmarkOrderItemValidator : CrabValidator<BenchmarkOrderItem>
    {
        public BenchmarkOrderItemValidator()
        {
            RuleFor(x => x.Sku)
                .NotEmpty()
                .MinimumLength(3);

            RuleFor(x => x.Quantity)
                .GreaterThan(0);
        }
    }
}
