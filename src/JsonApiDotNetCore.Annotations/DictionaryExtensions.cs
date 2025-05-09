namespace JsonApiDotNetCore;

internal static class DictionaryExtensions
{
    public static bool DictionaryEqual<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue>? first, IReadOnlyDictionary<TKey, TValue>? second,
        IEqualityComparer<TValue>? valueComparer = null)
    {
        if (ReferenceEquals(first, second))
        {
            return true;
        }

        if (first == null || second == null)
        {
            return false;
        }

        if (first.Count != second.Count)
        {
            return false;
        }

        IEqualityComparer<TValue> effectiveValueComparer = valueComparer ?? EqualityComparer<TValue>.Default;

        foreach ((TKey firstKey, TValue firstValue) in first)
        {
            if (!second.TryGetValue(firstKey, out TValue? secondValue))
            {
                return false;
            }

            if (!effectiveValueComparer.Equals(firstValue, secondValue))
            {
                return false;
            }
        }

        return true;
    }
}
