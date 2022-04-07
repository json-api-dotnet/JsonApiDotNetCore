#pragma warning disable AV1008 // Class should not be static
#pragma warning disable AV1130 // Return type in method signature should be an interface to an unchangeable collection

namespace JsonApiDotNetCore;

internal static class ArrayFactory
{
    public static T[] Create<T>(params T[] items)
    {
        return items;
    }
}
