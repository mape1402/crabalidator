namespace Crabalidator.Benchmarks
{
    public sealed class BenchmarkRegistrationRequestValidator : CrabValidator<BenchmarkRegistrationRequest>
    {
        public BenchmarkRegistrationRequestValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty()
                .MustAsync((value, _) => new ValueTask<bool>(value != "taken"));

            RuleFor(x => x.Email)
                .NotNull()
                .MaximumLength(40);
        }
    }
}
