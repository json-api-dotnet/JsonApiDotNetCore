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

    public static int FindIndex<T>(this IReadOnlyList<T> source, T item)
    {
        ArgumentNullException.ThrowIfNull(source);

        for (int index = 0; index < source.Count; index++)
        {
            if (EqualityComparer<T>.Default.Equals(source[index], item))
            {
                return index;
            }
        }

        return -1;
    }

    public static int FindIndex<T>(this IReadOnlyList<T> source, Predicate<T> match)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(match);

        for (int index = 0; index < source.Count; index++)
        {
            if (match(source[index]))
            {
                return index;
            }
        }

        return -1;
    }

    public static IEnumerable<T> ToEnumerable<T>(this LinkedListNode<T>? startNode)
    {
        LinkedListNode<T>? current = startNode;

        while (current != null)
        {
            yield return current.Value;

            current = current.Next;
        }
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
        return source ?? Array.Empty<T>();
    }

    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
    {
#pragma warning disable AV1250 // Evaluate LINQ query before returning it
        return source.Where(element => element is not null)!;
#pragma warning restore AV1250 // Evaluate LINQ query before returning it
    }
}
