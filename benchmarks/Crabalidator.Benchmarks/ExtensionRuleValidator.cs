namespace Crabalidator.Benchmarks
{
    public sealed class ExtensionRuleValidator : CrabValidator<ExtensionRuleRequest>
    {
        public ExtensionRuleValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MinimumLength(3)
                .MaximumLength(12)
                .Matches("^CRAB")
                .StartsWith("CR")
                .EndsWith("ID")
                .Contains("AB");

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
                .MinimumCount(2)
                .MaximumCount(5);

            RuleFor(x => x.Status)
                .In("ACTIVE", "PENDING")
                .NotIn("BLOCKED");

            RuleFor(x => x.Optional)
                .Null();
        }
    }
}
