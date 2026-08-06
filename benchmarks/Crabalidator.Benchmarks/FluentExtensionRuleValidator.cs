using FluentValidation;

namespace Crabalidator.Benchmarks
{
    public sealed class FluentExtensionRuleValidator : AbstractValidator<ExtensionRuleRequest>
    {
        public FluentExtensionRuleValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MinimumLength(3)
                .MaximumLength(12)
                .Matches("^CRAB")
                .Must(value => value == null || value.StartsWith("CR", StringComparison.Ordinal))
                .Must(value => value == null || value.EndsWith("ID", StringComparison.Ordinal))
                .Must(value => value == null || value.Contains("AB", StringComparison.Ordinal));

            RuleFor(x => x.Email)
                .NotNull()
                .EmailAddress();

            RuleFor(x => x.Age)
                .NotEmpty()
                .InclusiveBetween(18, 99);

            RuleFor(x => x.Score)
                .GreaterThan(0)
                .LessThanOrEqualTo(100);

            RuleFor(x => x.Items)
                .NotEmpty()
                .Must(value => value == null || value.Count >= 2)
                .Must(value => value == null || value.Count <= 5);

            RuleFor(x => x.Status)
                .Must(value => value is "ACTIVE" or "PENDING")
                .Must(value => value != "BLOCKED");

            RuleFor(x => x.Optional)
                .Null();
        }
    }
}
