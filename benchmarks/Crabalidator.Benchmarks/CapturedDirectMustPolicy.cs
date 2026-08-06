namespace Crabalidator.Benchmarks
{
    public sealed class CapturedDirectMustPolicy
    {
        private readonly string _prefix;

        public CapturedDirectMustPolicy(string prefix)
        {
            _prefix = prefix;
        }

        public bool HasValidCode(string value)
            => value != null && value.StartsWith(_prefix, StringComparison.Ordinal);
    }
}
