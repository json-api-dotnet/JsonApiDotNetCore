#pragma warning disable AV1130 // Return type in method signature should be an interface to an unchangeable collection

namespace JsonApiDotNetCore;

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

    public static HashSet<T> AsHashSet<T>(this T element)
    {
        return new HashSet<T>
        {
            element
        };
    }
}
