namespace Crabalidator.Tests
{
    public class NestedValidatorResolutionTests
    {
        [Fact]
        public void Inferred_Nested_Validators_Fall_Back_To_Single_Assembly_Candidate_Without_Di()
        {
            var validator = new FallbackOrderValidator();

            var result = validator.Validate(new FallbackOrder
            {
                Child = new FallbackChild { Name = "" },
                Items = new List<FallbackItem>
                {
                    new FallbackItem { Sku = "" }
                }
            });

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, x => x.PropertyPath == "Child.Name");
            Assert.Contains(result.Errors, x => x.PropertyPath == "Items[0].Sku");
        }

        [Fact]
        public void Inferred_Nested_Validators_Throw_When_Multiple_Assembly_Candidates_Exist()
        {
            var validator = new AmbiguousOrderValidator();

            var exception = Assert.Throws<InvalidOperationException>(() => validator.Validate(new AmbiguousOrder
            {
                Child = new AmbiguousChild()
            }));

            Assert.Contains("Multiple validators", exception.Message);
            Assert.Contains(typeof(AmbiguousChild).FullName, exception.Message);
        }

        [Fact]
        public void Explicit_Nested_Validator_Type_Resolves_Ambiguous_Object_And_Collection_Validators()
        {
            var validator = new ExplicitAmbiguousOrderValidator();

            var result = validator.Validate(new ExplicitAmbiguousOrder
            {
                Child = new AmbiguousChild { Name = "" },
                Items = new List<AmbiguousItem>
                {
                    new AmbiguousItem { Sku = "" }
                }
            });

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, x => x.PropertyPath == "Child.Name");
            Assert.Contains(result.Errors, x => x.PropertyPath == "Items[0].Sku");
        }

        private sealed class FallbackOrderValidator : CrabValidator<FallbackOrder>
        {
            public FallbackOrderValidator()
            {
                ValidateNested(x => x.Child);
                ValidateEach(x => x.Items);
            }
        }

        private sealed class FallbackChildValidator : CrabValidator<FallbackChild>
        {
            public FallbackChildValidator()
            {
                RuleFor(x => x.Name).NotEmpty();
            }
        }

        private sealed class FallbackItemValidator : CrabValidator<FallbackItem>
        {
            public FallbackItemValidator()
            {
                RuleFor(x => x.Sku).NotEmpty();
            }
        }

        private sealed class AmbiguousOrderValidator : CrabValidator<AmbiguousOrder>
        {
            public AmbiguousOrderValidator()
            {
                ValidateNested(x => x.Child);
            }
        }

        private sealed class ExplicitAmbiguousOrderValidator : CrabValidator<ExplicitAmbiguousOrder>
        {
            public ExplicitAmbiguousOrderValidator()
            {
                ValidateNestedWith<PrimaryAmbiguousChildValidator>(x => x.Child);
                ValidateEachWith<PrimaryAmbiguousItemValidator>(x => x.Items);
            }
        }

        private sealed class PrimaryAmbiguousChildValidator : CrabValidator<AmbiguousChild>
        {
            public PrimaryAmbiguousChildValidator()
            {
                RuleFor(x => x.Name).NotEmpty();
            }
        }

        private sealed class SecondaryAmbiguousChildValidator : CrabValidator<AmbiguousChild>
        {
            public SecondaryAmbiguousChildValidator()
            {
                RuleFor(x => x.Name).MinimumLength(3);
            }
        }

        private sealed class PrimaryAmbiguousItemValidator : CrabValidator<AmbiguousItem>
        {
            public PrimaryAmbiguousItemValidator()
            {
                RuleFor(x => x.Sku).NotEmpty();
            }
        }

        private sealed class SecondaryAmbiguousItemValidator : CrabValidator<AmbiguousItem>
        {
            public SecondaryAmbiguousItemValidator()
            {
                RuleFor(x => x.Sku).MinimumLength(3);
            }
        }

        private sealed class FallbackOrder
        {
            public FallbackChild Child { get; set; }

            public List<FallbackItem> Items { get; set; }
        }

        private sealed class FallbackChild
        {
            public string Name { get; set; }
        }

        private sealed class FallbackItem
        {
            public string Sku { get; set; }
        }

        private sealed class AmbiguousOrder
        {
            public AmbiguousChild Child { get; set; }
        }

        private sealed class ExplicitAmbiguousOrder
        {
            public AmbiguousChild Child { get; set; }

            public List<AmbiguousItem> Items { get; set; }
        }

        private sealed class AmbiguousChild
        {
            public string Name { get; set; }
        }

        private sealed class AmbiguousItem
        {
            public string Sku { get; set; }
        }
    }
}
