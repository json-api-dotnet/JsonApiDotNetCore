#pragma warning disable AV1008 // Class should not be static
#pragma warning disable AV1130 // Return type in method signature should be a collection interface instead of a concrete type

namespace JsonApiDotNetCore
{
    internal static class ArrayFactory
    {
        public static T[] Create<T>(params T[] items)
        {
            return items;
        }
    }
}
