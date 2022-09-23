using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace JsonApiDotNetCore;

internal static class CollectionExtensions
{
    [Pure]
    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this IEnumerable<T>? source)
    {
        if (source == null)
        {
            return true;
        }

        return !source.Any();
    }

    public static int FindIndex<T>(this IReadOnlyList<T> source, Predicate<T> match)
    {
        ArgumentGuard.NotNull(source);
        ArgumentGuard.NotNull(match);

        for (int index = 0; index < source.Count; index++)
        {
            if (match(source[index]))
            {
                return index;
            }
        }

        return -1;
    }

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

    public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? source)
    {
        return source ?? Enumerable.Empty<T>();
    }

    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
    {
#pragma warning disable AV1250 // Evaluate LINQ query before returning it
        return source.Where(element => element is not null)!;
#pragma warning restore AV1250 // Evaluate LINQ query before returning it
    }

    public static void AddRange<T>(this ICollection<T> source, IEnumerable<T> itemsToAdd)
    {
        ArgumentGuard.NotNull(source);
        ArgumentGuard.NotNull(itemsToAdd);

        foreach (T item in itemsToAdd)
        {
            source.Add(item);
        }
    }
}
