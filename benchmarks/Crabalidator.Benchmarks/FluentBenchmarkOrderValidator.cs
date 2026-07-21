using FluentValidation;

namespace Crabalidator.Benchmarks
{
    public sealed class FluentBenchmarkOrderValidator : AbstractValidator<BenchmarkOrder>
    {
        public FluentBenchmarkOrderValidator()
        {
            RuleFor(x => x.Customer)
                .SetValidator(new FluentBenchmarkCustomerValidator());

            RuleFor(x => x.Items)
                .NotEmpty();

            RuleForEach(x => x.Items)
                .SetValidator(new FluentBenchmarkOrderItemValidator());
        }
    }
}
