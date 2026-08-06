namespace Crabalidator.Benchmarks
{
    public sealed class PublicInstanceMustValidator : CrabValidator<PublicDirectMustRequest>
    {
        private readonly CapturedDirectMustPolicy _policy = new("CR");

        public PublicInstanceMustValidator()
        {
            RuleFor(x => x.Code).Must(_policy.HasValidCode);
        }
    }
}
