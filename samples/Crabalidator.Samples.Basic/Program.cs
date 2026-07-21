using Crabalidator;
using Crabalidator.Diagnostics;
using Crabalidator.Samples.Basic;

var validator = new CustomerValidator();

Console.WriteLine("Crabalidator basic sample");
Console.WriteLine();
Console.WriteLine(validator.Descriptor.DescribeConfiguration());

var validCustomer = new Customer
{
    Name = "Ada Lovelace",
    Age = 37,
    Email = "ada@example.com",
    Address = new Address { PostalCode = "12345" }
};

var invalidCustomer = new Customer
{
    Name = "",
    Age = 16,
    Email = "this-email-is-too-long@example.com",
    Address = new Address { PostalCode = "12" }
};

SampleResultPrinter.Print("Valid customer", validator.Validate(validCustomer));
SampleResultPrinter.Print("Invalid customer", validator.Validate(invalidCustomer));

Console.WriteLine();
Console.WriteLine($"Planned properties: {validator.Plan.Properties.Count}");
Console.WriteLine($"Requires async: {validator.Plan.RequiresAsync}");
Console.WriteLine($"Requires context: {validator.Plan.RequiresContext}");
