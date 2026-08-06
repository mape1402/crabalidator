using System.ComponentModel;

#pragma warning disable CS1591

namespace Crabalidator
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class NullRulePredicate<TProperty>
    {
        public static bool IsNull(TProperty value)
            => value == null;

        public static bool IsDefault(TProperty value)
            => EqualityComparer<TProperty>.Default.Equals(value, default);

        public static bool IsNotDefault(TProperty value)
            => !EqualityComparer<TProperty>.Default.Equals(value, default);
    }
}

#pragma warning restore CS1591
