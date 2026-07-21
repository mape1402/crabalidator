namespace Crabalidator.Samples.Basic
{
    public sealed class Customer
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public string Email { get; set; }

        public Address Address { get; set; } = new Address();

        public string Status { get; set; }

        public bool IsDraft { get; set; }
    }
}
