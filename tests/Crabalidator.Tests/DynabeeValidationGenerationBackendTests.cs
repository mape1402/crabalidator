using Crabalidator.Generation;
using Crabalidator.Generation.Dynabee;
using Crabalidator.Planning;

namespace Crabalidator.Tests
{
    public class DynabeeValidationGenerationBackendTests
    {
        [Fact]
        public void Compile_Generates_Validator_Type_With_Dynabee()
        {
            var validator = new BackendCustomerValidator();
            var backend = new DynabeeValidationGenerationBackend();

            var compiled = backend.Compile(validator.Plan);

            Assert.Equal("Dynabee", backend.Name);
            Assert.True(backend.Supports(validator.Plan));
            Assert.NotNull(compiled.Validator);
            Assert.NotEqual(typeof(BackendCustomerValidator), compiled.ValidatorType);
            Assert.Contains("BackendCustomerValidator", compiled.ValidatorType.Name);
        }

        [Fact]
        public void Compiled_Invoker_Returns_Validation_Failures()
        {
            var validator = new BackendCustomerValidator();
            var backend = new DynabeeValidationGenerationBackend();
            var compiled = backend.Compile(validator.Plan);

            var result = compiled.Invoker.Invoke(new BackendCustomer
            {
                Name = "",
                Age = 15
            });

            Assert.False(result.IsValid);
            Assert.Equal(new[] { "Name", "Age" }, result.Errors.Select(x => x.PropertyPath));
        }

        [Fact]
        public void Compiled_Typed_Invoker_Uses_Typed_Fast_Path()
        {
            var validator = new BackendCustomerValidator();
            var backend = new DynabeeValidationGenerationBackend();
            var compiled = backend.Compile(validator.Plan);

            var typedInvoker = Assert.IsAssignableFrom<ICompiledValidatorInvoker<BackendCustomer>>(compiled.TypedInvoker);
            var result = typedInvoker.Invoke(new BackendCustomer
            {
                Name = "Ada",
                Age = 42
            });

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Registry_Caches_Compiled_Validator()
        {
            var validator = new BackendCustomerValidator();
            var registry = new Runtime.CompiledValidatorRegistry(
                new ValidationPlanBuilder(),
                new DynabeeValidationGenerationBackend());

            var first = registry.GetOrAdd(validator);
            var second = registry.GetOrAdd(validator);

            Assert.Same(first, second);
        }

        private sealed class BackendCustomerValidator : CrabValidator<BackendCustomer>
        {
            public BackendCustomerValidator()
            {
                RuleFor(x => x.Name).NotEmpty();
                RuleFor(x => x.Age).GreaterThanOrEqualTo(18);
            }
        }

        private sealed class BackendCustomer
        {
            public string Name { get; set; }

            public int Age { get; set; }
        }
    }
}
