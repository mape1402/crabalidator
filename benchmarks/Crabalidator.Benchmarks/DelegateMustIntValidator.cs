namespace Crabalidator.Benchmarks
{
    public sealed class DelegateMustIntValidator : CrabValidator<DelegateMustIntRequest>
    {
        public DelegateMustIntValidator()
        {
            RuleFor(x => x.Age)
                .Must(x => x >= 18);
        }
    }
}
