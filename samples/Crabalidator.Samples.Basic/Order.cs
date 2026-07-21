namespace Crabalidator.Samples.Basic
{
    public sealed class Order
    {
        public Customer Customer { get; set; }

        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
