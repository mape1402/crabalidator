using Crabalidator.Planning;

namespace Crabalidator.Generation
{
    /// <summary>
    /// Represents a compiled validator implementation.
    /// </summary>
    public sealed class CompiledValidator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompiledValidator"/> class.
        /// </summary>
        /// <param name="validator">The generated validator instance.</param>
        /// <param name="validatorType">The generated validator type.</param>
        /// <param name="invoker">The untyped invoker.</param>
        /// <param name="typedInvoker">The typed invoker.</param>
        /// <param name="plan">The validation plan.</param>
        public CompiledValidator(
            object validator,
            Type validatorType,
            ICompiledValidatorInvoker invoker,
            object typedInvoker,
            ValidationPlan plan)
        {
            Validator = validator ?? throw new ArgumentNullException(nameof(validator));
            ValidatorType = validatorType ?? throw new ArgumentNullException(nameof(validatorType));
            Invoker = invoker ?? throw new ArgumentNullException(nameof(invoker));
            TypedInvoker = typedInvoker;
            Plan = plan ?? throw new ArgumentNullException(nameof(plan));
        }

        /// <summary>
        /// Gets the generated validator instance.
        /// </summary>
        public object Validator { get; }

        /// <summary>
        /// Gets the generated validator type.
        /// </summary>
        public Type ValidatorType { get; }

        /// <summary>
        /// Gets the untyped invoker.
        /// </summary>
        public ICompiledValidatorInvoker Invoker { get; }

        /// <summary>
        /// Gets the typed invoker.
        /// </summary>
        public object TypedInvoker { get; }

        /// <summary>
        /// Gets the validation plan.
        /// </summary>
        public ValidationPlan Plan { get; }
    }
}
