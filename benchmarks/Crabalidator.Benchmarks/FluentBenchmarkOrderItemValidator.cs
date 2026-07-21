using FluentValidation;

namespace Crabalidator.Benchmarks
{
    public sealed class FluentBenchmarkOrderItemValidator : AbstractValidator<BenchmarkOrderItem>
    {
        public FluentBenchmarkOrderItemValidator()
        {
            RuleFor(x => x.Sku)
                .NotEmpty()
                .MinimumLength(3);

            RuleFor(x => x.Quantity)
                .GreaterThan(0);
        }
    }
}
