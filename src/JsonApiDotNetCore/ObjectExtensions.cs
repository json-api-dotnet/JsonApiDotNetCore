using System.Collections.Generic;

namespace JsonApiDotNetCore
{
    internal static class ObjectExtensions
    {
        public static IEnumerable<T> AsEnumerable<T>(this T element)
        {
            yield return element;
        }

        public static T[] AsArray<T>(this T element)
        {
            return new[]
            {
                element
            };
        }

        public static List<T> AsList<T>(this T element)
        {
            return new List<T>
            {
                element
            };
        }
    }
}
