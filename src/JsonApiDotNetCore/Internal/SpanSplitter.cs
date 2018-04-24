using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JsonApiDotNetCore.Extensions;

namespace JsonApiDotNetCore.Internal
{
    public readonly ref struct SpanSplitter
    {
        private readonly ReadOnlySpan<char> _span;
        private readonly List<int> _delimeterIndexes;
        private readonly List<Tuple<int, int>> _substringIndexes;

        public int Count => _substringIndexes.Count();
        public ReadOnlySpan<char> this[int index] => GetSpanForSubstring(index + 1);

        public SpanSplitter(ref string str, char delimeter)
        {
            _span = str.AsSpan();
            _delimeterIndexes = str.IndexesOf(delimeter).ToList();
            _substringIndexes = new List<Tuple<int, int>>();
            BuildSubstringIndexes();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => throw new NotSupportedException();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => throw new NotSupportedException();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => throw new NotSupportedException();

        private ReadOnlySpan<char> GetSpanForSubstring(int substringNumber)
        {
            if (substringNumber > Count)
            {
                throw new ArgumentOutOfRangeException($"There are only {Count} substrings given the delimeter and base string provided");
            }

            var indexes = _substringIndexes[substringNumber - 1];
            return _span.Slice(indexes.Item1, indexes.Item2);
        }
        
        private void BuildSubstringIndexes()
        {
            var start = 0;
            var end = 0;
            foreach (var index in _delimeterIndexes)
            {
                end = index;
                if (start > end) break;
                _substringIndexes.Add(new Tuple<int, int>(start, end - start));
                start = ++end;
            }

            if (end <= _span.Length)
            {
                _substringIndexes.Add(new Tuple<int, int>(start, _span.Length - start));
            }
        }
    }
}
