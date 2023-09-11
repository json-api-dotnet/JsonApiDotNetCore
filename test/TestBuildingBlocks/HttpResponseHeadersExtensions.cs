using System.Net.Http.Headers;
using Microsoft.Extensions.Primitives;

namespace TestBuildingBlocks;

public static class HttpResponseHeadersExtensions
{
    /// <summary>
    /// Returns the value of the specified HTTP response header, or <c>null</c> when not found. If the header occurs multiple times, their values are
    /// collapsed into a comma-separated string, without changing any surrounding double quotes.
    /// </summary>
    public static string? GetValue(this HttpResponseHeaders responseHeaders, string name)
    {
        if (responseHeaders.TryGetValues(name, out IEnumerable<string>? values))
        {
            return new StringValues(values.ToArray());
        }

        return null;
    }
}
