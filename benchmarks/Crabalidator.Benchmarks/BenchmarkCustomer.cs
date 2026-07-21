namespace Crabalidator.Benchmarks
{
    public sealed class BenchmarkCustomer
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public string Email { get; set; }

        public BenchmarkAddress Address { get; set; } = new BenchmarkAddress();

        public string Status { get; set; }

        public string Tier { get; set; }

        public string ReferralCode { get; set; }

        public int RiskScore { get; set; }
    }
}
