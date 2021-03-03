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
