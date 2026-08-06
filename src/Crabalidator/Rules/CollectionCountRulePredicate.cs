using System.Collections;
using System.ComponentModel;

#pragma warning disable CS1591

namespace Crabalidator
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class CollectionCountRulePredicate<TCollection>
        where TCollection : IEnumerable
    {
        private readonly int _minimum;
        private readonly int _maximum;

        public CollectionCountRulePredicate(int minimum, int maximum)
        {
            _minimum = minimum;
            _maximum = maximum;
        }

        public bool IsEmpty(TCollection value)
            => value == null || GetCount(value) == 0;

        public bool HasCount(TCollection value)
            => value == null || GetCount(value) == _minimum;

        public bool HasMinimumCount(TCollection value)
            => value == null || GetCount(value) >= _minimum;

        public bool HasMaximumCount(TCollection value)
            => value == null || GetCount(value) <= _maximum;

        public bool IsBetween(TCollection value)
        {
            if (value == null)
            {
                return true;
            }

            var count = GetCount(value);
            return count >= _minimum && count <= _maximum;
        }

        private static int GetCount(IEnumerable enumerable)
        {
            if (enumerable is ICollection collection)
            {
                return collection.Count;
            }

            var count = 0;
            var enumerator = enumerable.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    count++;
                }
            }
            finally
            {
                (enumerator as IDisposable)?.Dispose();
            }

            return count;
        }
    }
}

#pragma warning restore CS1591
