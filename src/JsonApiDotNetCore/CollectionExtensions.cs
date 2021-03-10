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

        public static int FindIndex<T>(this IList<T> source, Predicate<T> match)
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
    }
}
