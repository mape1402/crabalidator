using FluentValidation;

namespace Crabalidator.Benchmarks
{
    public sealed class FluentPublicDirectMustValidator : AbstractValidator<PublicDirectMustRequest>
    {
        public FluentPublicDirectMustValidator()
        {
            RuleFor(x => x.Code).Must(PublicDirectMustRules.HasValidCode);
        }
    }
}
