#pragma warning disable AV1008 // Class should not be static

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
