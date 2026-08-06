namespace Crabalidator.Benchmarks
{
    public sealed class LambdaFallbackMustValidator : CrabValidator<PublicDirectMustRequest>
    {
        public LambdaFallbackMustValidator()
        {
            RuleFor(x => x.Code).Must(value => value != null && value.StartsWith("CR", StringComparison.Ordinal));
        }
    }
}
