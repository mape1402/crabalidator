using FluentValidation;

namespace Crabalidator.Benchmarks
{
    public sealed class FluentDelegateConditionValidator : AbstractValidator<DelegateConditionRequest>
    {
        public FluentDelegateConditionValidator()
        {
            RuleFor(x => x.Age)
                .GreaterThanOrEqualTo(18)
                .When(x => x.ValidateAge);

            RuleFor(x => x.Status)
                .NotEmpty()
                .Unless(x => x.IsDraft);
        }
    }
}
