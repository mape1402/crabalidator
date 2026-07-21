using System.Text;
using Crabalidator.Configuration;

namespace Crabalidator.Diagnostics
{
    /// <summary>
    /// Diagnostic helpers for validator descriptors.
    /// </summary>
    public static class ValidatorDescriptorExtensions
    {
        /// <summary>
        /// Describes a configured validator.
        /// </summary>
        /// <param name="descriptor">The validator descriptor.</param>
        /// <returns>A readable configuration description.</returns>
        public static string DescribeConfiguration(this ValidatorDescriptor descriptor)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            var builder = new StringBuilder();
            builder.AppendLine($"Validator: {descriptor.ValidatorType.FullName}");
            builder.AppendLine($"Model: {descriptor.ModelType.FullName}");

            foreach (var propertyRule in descriptor.Rules)
            {
                builder.AppendLine($"- {propertyRule.PropertyPath} ({propertyRule.PropertyType.Name})");

                foreach (var rule in propertyRule.Rules)
                {
                    builder.AppendLine($"  - {rule.Kind}");
                }
            }

            return builder.ToString();
        }
    }
}
