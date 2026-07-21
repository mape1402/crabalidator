namespace Crabalidator.Benchmarks
{
    public static class BenchmarkOrderFactory
    {
        public static BenchmarkOrder CreateInvalid()
            => new BenchmarkOrder
            {
                Customer = BenchmarkCustomerFactory.CreateInvalid(),
                Items = new List<BenchmarkOrderItem>
                {
                    new BenchmarkOrderItem { Sku = "ABC-123", Quantity = 1 },
                    new BenchmarkOrderItem { Sku = string.Empty, Quantity = 0 }
                }
            };
    }
}
