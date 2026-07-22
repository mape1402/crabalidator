namespace Crabalidator.Benchmarks
{
    public sealed class DelegateMustStringValidator : CrabValidator<DelegateMustStringRequest>
    {
        public DelegateMustStringValidator()
        {
            RuleFor(x => x.Status)
                .Must(x => x == "ACTIVE");
        }
    }
}
