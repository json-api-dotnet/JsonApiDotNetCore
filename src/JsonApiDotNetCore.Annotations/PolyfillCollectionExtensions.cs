#if NET6_0
using System.Collections.ObjectModel;
#endif

#if NET6_0
#pragma warning disable AV1130 // Return type in method signature should be an interface to an unchangeable collection
#endif

namespace JsonApiDotNetCore;

// These methods provide polyfills for lower .NET versions.
internal static class PolyfillCollectionExtensions
{
    public static IReadOnlySet<T> AsReadOnly<T>(this HashSet<T> source)
    {
        // We can't use ReadOnlySet<T> yet, which is being introduced in .NET 9.
        return source;
    }

#if NET6_0
    public static ReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> source)
        where TKey : notnull
    {
        // The AsReadOnly() extension method is unavailable in .NET 6.
        return new ReadOnlyDictionary<TKey, TValue>(source);
    }

    public static ReadOnlyCollection<T> AsReadOnly<T>(this T[] source)
    {
        // The AsReadOnly() extension method is unavailable in .NET 6.
        return Array.AsReadOnly(source);
    }
#endif
}
