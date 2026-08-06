using System.ComponentModel;

#pragma warning disable CS1591

namespace Crabalidator
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class MembershipRulePredicate<TProperty>
    {
        private readonly TProperty[] _values;

        public MembershipRulePredicate(TProperty[] values)
        {
            _values = values ?? throw new ArgumentNullException(nameof(values));
        }

        public bool Contains(TProperty value)
            => _values.Contains(value);

        public bool DoesNotContain(TProperty value)
            => !_values.Contains(value);
    }
}

#pragma warning restore CS1591
