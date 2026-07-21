namespace Crabalidator.Samples.Basic
{
    public sealed class OrderItemValidator : CrabValidator<OrderItem>
    {
        public OrderItemValidator()
        {
            RuleFor(x => x.Sku)
                .NotEmpty()
                .MinimumLength(3);

            RuleFor(x => x.Quantity)
                .GreaterThan(0);
        }
    }
}
