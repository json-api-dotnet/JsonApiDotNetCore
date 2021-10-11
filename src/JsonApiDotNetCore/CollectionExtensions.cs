#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace JsonApiDotNetCore
{
    internal static class CollectionExtensions
    {
        [Pure]
        [ContractAnnotation("source: null => true")]
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
        {
            if (source == null)
            {
                return true;
            }

            return !source.Any();
        }

        public static int FindIndex<T>(this IReadOnlyList<T> source, Predicate<T> match)
        {
            ArgumentGuard.NotNull(source, nameof(source));
            ArgumentGuard.NotNull(match, nameof(match));

            for (int index = 0; index < source.Count; index++)
            {
                if (match(source[index]))
                {
                    return index;
                }
            }

            return -1;
        }

        public static bool DictionaryEqual<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> first, IReadOnlyDictionary<TKey, TValue> second,
            IEqualityComparer<TValue> valueComparer = null)
        {
            if (first == second)
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
                if (!second.TryGetValue(firstKey, out TValue secondValue))
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

        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> source)
        {
            return source ?? Enumerable.Empty<T>();
        }

        public static void AddRange<T>(this ICollection<T> source, IEnumerable<T> itemsToAdd)
        {
            ArgumentGuard.NotNull(source, nameof(source));
            ArgumentGuard.NotNull(itemsToAdd, nameof(itemsToAdd));

            foreach (T item in itemsToAdd)
            {
                source.Add(item);
            }
        }
    }
}
