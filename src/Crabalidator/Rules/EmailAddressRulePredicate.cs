using System.ComponentModel;
using System.Text.RegularExpressions;

#pragma warning disable CS1591

namespace Crabalidator
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class EmailAddressRulePredicate
    {
        private const string BasicEmailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

        public static bool IsValid(string value)
            => value == null || Regex.IsMatch(value, BasicEmailPattern);
    }
}

#pragma warning restore CS1591
