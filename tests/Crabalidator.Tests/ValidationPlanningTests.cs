using Crabalidator.Configuration;
using Crabalidator.Planning;

namespace Crabalidator.Tests
{
    public class ValidationPlanningTests
    {
        [Fact]
        public void Build_Creates_Deterministic_Property_And_Rule_Order()
        {
            var validator = new PlanningCustomerValidator();

            var plan = new ValidationPlanBuilder().Build(validator.Descriptor);

            Assert.Equal(typeof(PlanningCustomerValidator), plan.ValidatorType);
            Assert.Equal(typeof(PlanningCustomer), plan.ModelType);
            Assert.False(plan.RequiresAsync);
            Assert.False(plan.RequiresContext);
            Assert.Empty(plan.Diagnostics);

            Assert.Equal(new[] { "Name", "Age", "Address.PostalCode" }, plan.Properties.Select(x => x.PropertyPath));
            Assert.Equal(new[] { 0, 1, 2 }, plan.Properties.Select(x => x.Order));

            var namePlan = plan.Properties[0];
            Assert.Equal(new[] { RuleKind.NotEmpty, RuleKind.MaximumLength }, namePlan.Rules.Select(x => x.Kind));
            Assert.Equal(new[] { 0, 1 }, namePlan.Rules.Select(x => x.Order));
        }

        [Fact]
        public void Build_Carries_Failure_Plan_Metadata()
        {
            var validator = new PlanningCustomerValidator();

            var plan = validator.Plan;
            var ageRule = plan.Properties.Single(x => x.PropertyPath == "Age").Rules.Single();

            Assert.Equal(RuleKind.GreaterThanOrEqualTo, ageRule.Kind);
            Assert.Equal(18, ageRule.ComparisonValue);
            Assert.Null(ageRule.Minimum);
            Assert.Null(ageRule.Maximum);
            Assert.Equal("Age", ageRule.Failure.PropertyName);
            Assert.Equal("Age", ageRule.Failure.PropertyPath);
            Assert.Equal("'Age' must be greater than or equal to '18'.", ageRule.Failure.ErrorMessage);
            Assert.Equal(ValidationSeverity.Error, ageRule.Failure.Severity);
        }

        [Fact]
        public void Plan_Execution_Matches_Validator_Execution()
        {
            var validator = new PlanningCustomerValidator();
            var customer = new PlanningCustomer
            {
                Name = "",
                Age = 10,
                Address = new PlanningAddress { PostalCode = "12" }
            };

            var validatorResult = validator.Validate(customer);
            var planResult = ValidationPlanExecutor.Execute(validator.Plan, new ValidationContext<PlanningCustomer>(customer));

            Assert.Equal(validatorResult.Errors.Select(x => x.PropertyPath), planResult.Errors.Select(x => x.PropertyPath));
            Assert.Equal(validatorResult.Errors.Select(x => x.ErrorMessage), planResult.Errors.Select(x => x.ErrorMessage));
        }

        [Fact]
        public void Build_Adds_Diagnostic_For_Empty_Validator()
        {
            var validator = new EmptyPlanningValidator();

            var plan = validator.Plan;

            var diagnostic = Assert.Single(plan.Diagnostics);
            Assert.Equal("CRABPLAN001", diagnostic.Code);
            Assert.Equal(ValidationPlanDiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Null(diagnostic.PropertyPath);
        }

        [Fact]
        public void Build_Adds_Diagnostic_For_Property_Without_Rules()
        {
            var validator = new EmptyPropertyPlanningValidator();

            var plan = validator.Plan;

            var diagnostic = Assert.Single(plan.Diagnostics);
            Assert.Equal("CRABPLAN002", diagnostic.Code);
            Assert.Equal("Name", diagnostic.PropertyPath);
            Assert.Equal(ValidationPlanDiagnosticSeverity.Warning, diagnostic.Severity);
        }

        private sealed class PlanningCustomerValidator : CrabValidator<PlanningCustomer>
        {
            public PlanningCustomerValidator()
            {
                RuleFor(x => x.Name)
                    .NotEmpty()
                    .MaximumLength(10);

                RuleFor(x => x.Age)
                    .GreaterThanOrEqualTo(18);

                RuleFor(x => x.Address.PostalCode)
                    .Length(5, 5);
            }
        }

        private sealed class EmptyPlanningValidator : CrabValidator<PlanningCustomer>
        {
        }

        private sealed class EmptyPropertyPlanningValidator : CrabValidator<PlanningCustomer>
        {
            public EmptyPropertyPlanningValidator()
            {
                RuleFor(x => x.Name);
            }
        }

        private sealed class PlanningCustomer
        {
            public string Name { get; set; }

            public int Age { get; set; }

            public PlanningAddress Address { get; set; } = new PlanningAddress();
        }

        private sealed class PlanningAddress
        {
            public string PostalCode { get; set; }
        }
    }
}
