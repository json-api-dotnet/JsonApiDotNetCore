using System.Net.Http.Headers;
using Microsoft.Extensions.Primitives;

namespace TestBuildingBlocks;

public static class HttpRequestHeadersExtensions
{
    /// <summary>
    /// Returns the value of the specified HTTP request header, or <c>null</c> when not found. If the header occurs multiple times, their values are
    /// collapsed into a comma-separated string, without changing any surrounding double quotes.
    /// </summary>
    public static string? GetValue(this HttpRequestHeaders requestHeaders, string name)
    {
        return requestHeaders.TryGetValues(name, out IEnumerable<string>? values) ? new StringValues(values.ToArray()).ToString() : null;
    }
}
