using System.Collections.Immutable;

namespace JsonApiDotNetCore.SourceGenerators;

// This type was copied from Roslyn. The implementation looks odd, but is likely a performance tradeoff.
// Beware that the consuming code doesn't adhere to the typical pattern where a dictionary is built once, then queried many times.

internal sealed class ImmutableDictionaryEqualityComparer<TKey, TValue> : IEqualityComparer<ImmutableDictionary<TKey, TValue>?>
    where TKey : notnull
{
    public static readonly ImmutableDictionaryEqualityComparer<TKey, TValue> Instance = new();

    public bool Equals(ImmutableDictionary<TKey, TValue>? x, ImmutableDictionary<TKey, TValue>? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (x is null || y is null)
        {
            return false;
        }

        if (!Equals(x.KeyComparer, y.KeyComparer) || !Equals(x.ValueComparer, y.ValueComparer))
        {
            return false;
        }

        foreach ((TKey key, TValue value) in x)
        {
            if (!y.TryGetValue(key, out TValue? other) || !x.ValueComparer.Equals(value, other))
            {
                return false;
            }
        }

        return true;
    }

    public int GetHashCode(ImmutableDictionary<TKey, TValue>? obj)
    {
        return obj?.Count ?? 0;
    }
}
