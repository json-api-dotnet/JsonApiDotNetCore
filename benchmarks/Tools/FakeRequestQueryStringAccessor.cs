using JsonApiDotNetCore.QueryStrings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace Benchmarks.Tools;

/// <summary>
/// Enables to inject a query string, instead of obtaining it from <see cref="HttpContext" />.
/// </summary>
internal sealed class FakeRequestQueryStringAccessor : IRequestQueryStringAccessor
{
    public IQueryCollection Query { get; private set; } = new QueryCollection();

    public void SetQueryString(string queryString)
    {
        Query = new QueryCollection(QueryHelpers.ParseQuery(queryString));
    }
}
