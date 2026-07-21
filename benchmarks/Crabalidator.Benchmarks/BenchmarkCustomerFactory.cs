namespace Crabalidator.Benchmarks
{
    public static class BenchmarkCustomerFactory
    {
        public static BenchmarkCustomer CreateValid()
            => new BenchmarkCustomer
            {
                Name = "Ada Lovelace",
                Age = 37,
                Email = "ada@example.com",
                Address = new BenchmarkAddress
                {
                    PostalCode = "12345",
                    CountryCode = "MX"
                },
                Status = "ACTIVE",
                Tier = "PRO",
                ReferralCode = "WELCOME",
                RiskScore = 25
            };

        public static BenchmarkCustomer CreateInvalid()
            => new BenchmarkCustomer
            {
                Name = string.Empty,
                Age = 16,
                Email = "this-email-is-too-long-for-the-benchmark@example.com",
                Address = new BenchmarkAddress
                {
                    PostalCode = "12",
                    CountryCode = "US"
                },
                Status = "SUSPENDED",
                Tier = "FREE",
                ReferralCode = "BLOCKED",
                RiskScore = 99
            };
    }
}
