using System.ComponentModel;
using System.Text.RegularExpressions;

#pragma warning disable CS1591

namespace Crabalidator
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class StringPatternRulePredicate
    {
        private readonly Regex _regex;

        public StringPatternRulePredicate(string pattern)
            : this(new Regex(pattern, RegexOptions.None))
        {
        }

        public StringPatternRulePredicate(Regex regex)
        {
            _regex = regex ?? throw new ArgumentNullException(nameof(regex));
        }

        public bool IsMatch(string value)
            => value == null || _regex.IsMatch(value);
    }
}

#pragma warning restore CS1591
