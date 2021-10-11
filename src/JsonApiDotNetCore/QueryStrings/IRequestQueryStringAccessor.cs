using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.QueryStrings
{
    /// <summary>
    /// Provides access to the query string of a URL in a HTTP request.
    /// </summary>
    public interface IRequestQueryStringAccessor
    {
        IQueryCollection Query { get; }
    }
}
