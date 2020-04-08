using System;
using System.Collections;
using System.Collections.Generic;

namespace JsonApiDotNetCore.Extensions
{
    internal static class SystemCollectionExtensions
    {
        public static void AddRange<T>(this ICollection<T> source, IEnumerable<T> items)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (items == null) throw new ArgumentNullException(nameof(items));

            foreach (var item in items)
            {
                source.Add(item);
            }
        }

        public static void AddRange<T>(this IList source, IEnumerable<T> items)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (items == null) throw new ArgumentNullException(nameof(items));

            foreach (var item in items)
            {
                source.Add(item);
            }
        }
    }
}
