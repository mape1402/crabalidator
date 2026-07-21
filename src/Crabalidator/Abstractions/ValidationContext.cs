namespace Crabalidator
{
    /// <summary>
    /// Carries the instance being validated and contextual validation data.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    public sealed class ValidationContext<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationContext{T}"/> class.
        /// </summary>
        /// <param name="instanceToValidate">The instance to validate.</param>
        public ValidationContext(T instanceToValidate)
        {
            InstanceToValidate = instanceToValidate;
            RootContextData = new Dictionary<string, object>();
        }

        /// <summary>
        /// Gets the instance being validated.
        /// </summary>
        public T InstanceToValidate { get; }

        /// <summary>
        /// Gets user-defined context data available during validation.
        /// </summary>
        public IDictionary<string, object> RootContextData { get; }
    }
}
