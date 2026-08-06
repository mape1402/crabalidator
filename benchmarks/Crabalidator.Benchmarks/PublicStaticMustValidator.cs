namespace Crabalidator.Benchmarks
{
    public sealed class PublicStaticMustValidator : CrabValidator<PublicDirectMustRequest>
    {
        public PublicStaticMustValidator()
        {
            RuleFor(x => x.Code).Must(PublicDirectMustRules.HasValidCode);
        }
    }
}
