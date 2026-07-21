using Crabalidator;
using Crabalidator.DependencyInjection;
using Crabalidator.Diagnostics;
using Crabalidator.Samples.Basic;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddCrabalidator(typeof(CustomerValidator).Assembly);

var provider = services.BuildServiceProvider();
var crabalidator = provider.GetRequiredService<ICrabalidator>();
var customerValidator = provider.GetRequiredService<CrabValidator<Customer>>();
var orderValidator = provider.GetRequiredService<CrabValidator<Order>>();
var registrationValidator = provider.GetRequiredService<CrabValidator<RegistrationRequest>>();

Console.WriteLine("Crabalidator basic sample");
Console.WriteLine();
Console.WriteLine(customerValidator.Descriptor.DescribeConfiguration());

var validCustomer = new Customer
{
    Name = "Ada Lovelace",
    Age = 37,
    Email = "ada@example.com",
    Address = new Address
    {
        PostalCode = "12345",
        CountryCode = "MX",
        State = "JAL"
    },
    Status = "ACTIVE",
    Tier = "PRO",
    ReferralCode = "WELCOME",
    RiskScore = 25
};

var invalidCustomer = new Customer
{
    Name = "",
    Age = 16,
    Email = "this-email-is-too-long@example.com",
    Address = new Address
    {
        PostalCode = "12",
        CountryCode = "US",
        State = ""
    },
    Status = "SUSPENDED",
    Tier = "FREE",
    ReferralCode = "BLOCKED",
    RiskScore = 99
};

var invalidOrder = new Order
{
    Customer = invalidCustomer,
    Items = new List<OrderItem>
    {
        new OrderItem { Sku = "ABC-123", Quantity = 1 },
        new OrderItem { Sku = "", Quantity = 0 }
    }
};

var invalidRegistration = new RegistrationRequest
{
    Username = "taken",
    Email = "new@example.com"
};

SampleResultPrinter.Print("Valid customer", crabalidator.Validate(validCustomer));
SampleResultPrinter.Print("Invalid customer", crabalidator.Validate(invalidCustomer));
SampleResultPrinter.Print("Invalid order", crabalidator.Validate(invalidOrder));
SampleResultPrinter.Print("Invalid async registration", await crabalidator.ValidateAsync(invalidRegistration));

Console.WriteLine();
Console.WriteLine($"Customer planned properties: {customerValidator.Plan.Properties.Count}");
Console.WriteLine($"Order planned properties: {orderValidator.Plan.Properties.Count}");
Console.WriteLine($"Registration requires async: {registrationValidator.Plan.RequiresAsync}");
Console.WriteLine($"Customer requires context: {customerValidator.Plan.RequiresContext}");
