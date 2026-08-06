namespace Crabalidator.Benchmarks
{
    public static class PublicDirectMustRules
    {
        public static bool HasValidCode(string value)
            => value != null && value.StartsWith("CR", StringComparison.Ordinal);
    }
}
