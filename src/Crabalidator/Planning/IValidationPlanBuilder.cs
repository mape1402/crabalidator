using Crabalidator.Configuration;

namespace Crabalidator.Planning
{
    /// <summary>
    /// Builds validation plans from configured validator descriptors.
    /// </summary>
    public interface IValidationPlanBuilder
    {
        /// <summary>
        /// Builds a validation plan.
        /// </summary>
        /// <param name="descriptor">The validator descriptor.</param>
        /// <returns>The validation plan.</returns>
        ValidationPlan Build(ValidatorDescriptor descriptor);
    }
}
