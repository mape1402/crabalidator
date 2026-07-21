using Crabalidator;
using Crabalidator.DependencyInjection;
using Crabalidator.Diagnostics;
using Crabalidator.Samples.Basic;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddCrabalidator(typeof(CustomerValidator).Assembly);

var provider = services.BuildServiceProvider();
var crabalidator = provider.GetRequiredService<ICrabalidator>();
var configuredValidator = provider.GetRequiredService<CrabValidator<Customer>>();

Console.WriteLine("Crabalidator basic sample");
Console.WriteLine();
Console.WriteLine(configuredValidator.Descriptor.DescribeConfiguration());

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

SampleResultPrinter.Print("Valid customer", crabalidator.Validate(validCustomer));
SampleResultPrinter.Print("Invalid customer", crabalidator.Validate(invalidCustomer));

Console.WriteLine();
Console.WriteLine($"Planned properties: {configuredValidator.Plan.Properties.Count}");
Console.WriteLine($"Requires async: {configuredValidator.Plan.RequiresAsync}");
Console.WriteLine($"Requires context: {configuredValidator.Plan.RequiresContext}");
