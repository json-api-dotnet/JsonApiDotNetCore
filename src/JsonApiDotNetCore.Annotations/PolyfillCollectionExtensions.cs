using System.Collections.ObjectModel;

namespace JsonApiDotNetCore;

// These methods provide polyfills for lower .NET versions.
internal static class PolyfillCollectionExtensions
{
    public static IReadOnlySet<T> AsReadOnly<T>(this HashSet<T> source)
    {
        return new ReadOnlySet<T>(source);
    }
}
