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
    }
}
