namespace Crabalidator.Benchmarks
{
    public sealed class DelegateConditionValidator : CrabValidator<DelegateConditionRequest>
    {
        public DelegateConditionValidator()
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
