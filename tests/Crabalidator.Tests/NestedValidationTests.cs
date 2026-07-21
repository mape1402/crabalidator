using Crabalidator.Generation;
using Crabalidator.Generation.Dynabee;

namespace Crabalidator.Tests
{
    public class NestedValidationTests
    {
        [Fact]
        public void SetValidator_Prefixes_Child_Failure_Paths()
        {
            var validator = new OrderValidator();

            var result = validator.Validate(new Order
            {
                Customer = new Customer { Name = "" },
                Items = new List<OrderItem>()
            });

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, x => x.PropertyPath == "Customer.Name");
        }

        [Fact]
        public void RuleForEach_Prefixes_Collection_Item_Failure_Paths()
        {
            var validator = new OrderValidator();

            var result = validator.Validate(new Order
            {
                Customer = new Customer { Name = "Ada" },
                Items = new List<OrderItem>
                {
                    new OrderItem { Sku = "ABC", Quantity = 1 },
                    new OrderItem { Sku = "", Quantity = 0 }
                }
            });

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, x => x.PropertyPath == "Items[1].Sku");
            Assert.Contains(result.Errors, x => x.PropertyPath == "Items[1].Quantity");
        }

        [Fact]
        public void Nested_Validators_Skip_Null_Child_Values()
        {
            var validator = new OrderValidator();

            var result = validator.Validate(new Order
            {
                Customer = null,
                Items = null
            });

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Plan_Carries_Nested_Validation_Metadata()
        {
            var validator = new OrderValidator();

            var customer = validator.Plan.Properties.Single(x => x.PropertyPath == "Customer");
            var items = validator.Plan.Properties.Single(x => x.PropertyPath == "Items");

            Assert.NotNull(customer.NestedValidation);
            Assert.False(customer.NestedValidation.IsCollection);
            Assert.Equal(typeof(Customer), customer.NestedValidation.ModelType);

            Assert.NotNull(items.NestedValidation);
            Assert.True(items.NestedValidation.IsCollection);
            Assert.Equal(typeof(OrderItem), items.NestedValidation.ModelType);
        }

        [Fact]
        public void Dynabee_Backend_Executes_Nested_Validation_Plans()
        {
            var validator = new OrderValidator();
            var backend = new DynabeeValidationGenerationBackend();
            var compiled = backend.Compile(validator.Plan);
            var typedInvoker = Assert.IsAssignableFrom<ICompiledValidatorInvoker<Order>>(compiled.TypedInvoker);

            var result = typedInvoker.Invoke(new Order
            {
                Customer = new Customer { Name = "" },
                Items = new List<OrderItem>
                {
                    new OrderItem { Sku = "", Quantity = 0 }
                }
            });

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, x => x.PropertyPath == "Customer.Name");
            Assert.Contains(result.Errors, x => x.PropertyPath == "Items[0].Sku");
            Assert.Contains(result.Errors, x => x.PropertyPath == "Items[0].Quantity");
        }

        private sealed class OrderValidator : CrabValidator<Order>
        {
            public OrderValidator()
            {
                RuleFor(x => x.Customer)
                    .SetValidator(new CustomerValidator());

                RuleFor(x => x.Items)
                    .RuleForEach(new OrderItemValidator());
            }
        }

        private sealed class CustomerValidator : CrabValidator<Customer>
        {
            public CustomerValidator()
            {
                RuleFor(x => x.Name)
                    .NotEmpty()
                    .WithMessage("Customer name is required.");
            }
        }

        private sealed class OrderItemValidator : CrabValidator<OrderItem>
        {
            public OrderItemValidator()
            {
                RuleFor(x => x.Sku)
                    .NotEmpty();

                RuleFor(x => x.Quantity)
                    .GreaterThan(0);
            }
        }

        private sealed class Order
        {
            public Customer Customer { get; set; }

            public List<OrderItem> Items { get; set; }
        }

        private sealed class Customer
        {
            public string Name { get; set; }
        }

        private sealed class OrderItem
        {
            public string Sku { get; set; }

            public int Quantity { get; set; }
        }
    }
}
