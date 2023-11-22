#pragma warning disable AV1130 // Return type in method signature should be an interface to an unchangeable collection

namespace JsonApiDotNetCore;

internal static class ObjectExtensions
{
    public static HashSet<T> AsHashSet<T>(this T element)
    {
        return [element];
    }
}
