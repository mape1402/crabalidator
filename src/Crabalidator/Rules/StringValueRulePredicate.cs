using System.ComponentModel;

#pragma warning disable CS1591

namespace Crabalidator
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class StringValueRulePredicate
    {
        private readonly string _value;
        private readonly StringComparison _comparison;

        public StringValueRulePredicate(string value, StringComparison comparison)
        {
            _value = value ?? throw new ArgumentNullException(nameof(value));
            _comparison = comparison;
        }

        public bool StartsWith(string text)
            => text == null || text.StartsWith(_value, _comparison);

        public bool EndsWith(string text)
            => text == null || text.EndsWith(_value, _comparison);

        public bool Contains(string text)
            => text == null || text.Contains(_value, _comparison);
    }
}

#pragma warning restore CS1591
