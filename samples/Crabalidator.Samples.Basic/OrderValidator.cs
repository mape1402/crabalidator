namespace Crabalidator.Samples.Basic
{
    public sealed class OrderValidator : CrabValidator<Order>
    {
        public OrderValidator()
        {
            RuleFor(x => x.Customer)
                .SetValidator(new CustomerValidator());

            RuleFor(x => x.Items)
                .NotEmpty()
                .RuleForEach(new OrderItemValidator());
        }
    }
}
