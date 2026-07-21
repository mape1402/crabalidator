namespace Crabalidator.Benchmarks
{
    public sealed class BenchmarkOrder
    {
        public BenchmarkCustomer Customer { get; set; }

        public List<BenchmarkOrderItem> Items { get; set; } = new List<BenchmarkOrderItem>();
    }
}
