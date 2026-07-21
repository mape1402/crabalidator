using FluentValidation;

namespace Crabalidator.Benchmarks
{
    public sealed class FluentBenchmarkRegistrationRequestValidator : AbstractValidator<BenchmarkRegistrationRequest>
    {
        public FluentBenchmarkRegistrationRequestValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty()
                .MustAsync((value, _) => Task.FromResult(value != "taken"));

            RuleFor(x => x.Email)
                .NotNull()
                .MaximumLength(40);
        }
    }
}
