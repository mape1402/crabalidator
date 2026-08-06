using System.ComponentModel;

#pragma warning disable CS1591

namespace Crabalidator
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class ComparableRangeRulePredicate<TProperty>
        where TProperty : IComparable
    {
        private readonly TProperty _minimum;
        private readonly TProperty _maximum;

        public ComparableRangeRulePredicate(TProperty minimum, TProperty maximum)
        {
            _minimum = minimum;
            _maximum = maximum;
        }

        public bool IsInclusive(TProperty value)
            => value != null && value.CompareTo(_minimum) >= 0 && value.CompareTo(_maximum) <= 0;

        public bool IsExclusive(TProperty value)
            => value != null && value.CompareTo(_minimum) > 0 && value.CompareTo(_maximum) < 0;
    }
}

#pragma warning restore CS1591
