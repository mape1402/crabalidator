namespace Crabalidator.Testing
{
    /// <summary>
    /// Entry point for direct validator tests.
    /// </summary>
    public static class CrabalidatorTest
    {
        /// <summary>
        /// Creates a direct test runner for a validator.
        /// </summary>
        /// <typeparam name="TValidator">The validator type.</typeparam>
        /// <returns>A validator test runner.</returns>
        public static CrabalidatorTestRunner<TValidator> For<TValidator>()
            where TValidator : class
            => new CrabalidatorTestRunner<TValidator>();
    }
}
