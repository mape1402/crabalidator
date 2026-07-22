namespace Crabalidator.Samples.Basic
{
    public sealed class OrderValidator : CrabValidator<Order>
    {
        public OrderValidator()
        {
            ValidateNested(x => x.Customer);

            RuleFor(x => x.Items)
                .NotEmpty();

            ValidateEach(x => x.Items);
        }
    }
}
