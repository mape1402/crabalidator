namespace Crabalidator.Testing
{
    /// <summary>
    /// Exception thrown when a Crabalidator testing assertion fails.
    /// </summary>
    public sealed class CrabalidatorAssertionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CrabalidatorAssertionException"/> class.
        /// </summary>
        /// <param name="message">The assertion failure message.</param>
        public CrabalidatorAssertionException(string message)
            : base(message)
        {
        }
    }
}
