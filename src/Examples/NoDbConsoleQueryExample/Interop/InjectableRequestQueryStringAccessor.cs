using JsonApiDotNetCore.QueryStrings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace NoDbConsoleQueryExample.Interop;

/// <summary>
/// Enables to directly inject a query string, when not running inside an ASP.NET context.
/// </summary>
internal sealed class InjectableRequestQueryStringAccessor : IRequestQueryStringAccessor
{
    public IQueryCollection Query { get; private set; } = new QueryCollection();

    public void SetFromText(string queryString)
    {
        Query = new QueryCollection(QueryHelpers.ParseQuery(queryString));
    }
}
