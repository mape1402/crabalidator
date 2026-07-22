using FluentValidation;

namespace Crabalidator.Benchmarks
{
    public sealed class FluentDelegateMustStringValidator : AbstractValidator<DelegateMustStringRequest>
    {
        public FluentDelegateMustStringValidator()
        {
            RuleFor(x => x.Status)
                .Must(x => x == "ACTIVE");
        }
    }
}
