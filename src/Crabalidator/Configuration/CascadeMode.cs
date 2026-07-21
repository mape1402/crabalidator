namespace Crabalidator
{
    /// <summary>
    /// Controls rule execution after a failure.
    /// </summary>
    public enum CascadeMode
    {
        /// <summary>
        /// Continue evaluating remaining rules.
        /// </summary>
        Continue,

        /// <summary>
        /// Stop evaluating remaining rules for the current property after the first failure.
        /// </summary>
        Stop
    }
}
