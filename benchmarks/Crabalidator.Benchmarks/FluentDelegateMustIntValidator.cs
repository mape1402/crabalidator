using FluentValidation;

namespace Crabalidator.Benchmarks
{
    public sealed class FluentDelegateMustIntValidator : AbstractValidator<DelegateMustIntRequest>
    {
        public FluentDelegateMustIntValidator()
        {
            RuleFor(x => x.Age)
                .Must(x => x >= 18);
        }
    }
}
