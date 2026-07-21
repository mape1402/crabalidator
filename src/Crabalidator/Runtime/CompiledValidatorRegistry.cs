using System.Collections.Concurrent;
using Crabalidator.Configuration;
using Crabalidator.Generation;
using Crabalidator.Planning;

namespace Crabalidator.Runtime
{
    /// <summary>
    /// Caches compiled validators.
    /// </summary>
    public sealed class CompiledValidatorRegistry : ICompiledValidatorRegistry
    {
        private readonly ConcurrentDictionary<Type, Lazy<CompiledValidator>> _validators = new();
        private readonly IValidationPlanBuilder _planBuilder;
        private readonly IValidationGenerationBackend _generationBackend;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompiledValidatorRegistry"/> class.
        /// </summary>
        /// <param name="planBuilder">The validation plan builder.</param>
        /// <param name="generationBackend">The validation generation backend.</param>
        public CompiledValidatorRegistry(
            IValidationPlanBuilder planBuilder,
            IValidationGenerationBackend generationBackend)
        {
            _planBuilder = planBuilder ?? throw new ArgumentNullException(nameof(planBuilder));
            _generationBackend = generationBackend ?? throw new ArgumentNullException(nameof(generationBackend));
        }

        /// <inheritdoc/>
        public CompiledValidator GetOrAdd<T>(CrabValidator<T> validator)
        {
            if (validator == null)
            {
                throw new ArgumentNullException(nameof(validator));
            }

            return _validators.GetOrAdd(
                typeof(T),
                _ => new Lazy<CompiledValidator>(() => Compile(validator.Descriptor))).Value;
        }

        private CompiledValidator Compile(ValidatorDescriptor descriptor)
        {
            var plan = _planBuilder.Build(descriptor);
            return _generationBackend.Compile(plan);
        }
    }
}
