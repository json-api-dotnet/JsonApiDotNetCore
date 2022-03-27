using JsonApiDotNetCore.QueryStrings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace BackgroundWorkerService.Interop;

internal sealed class FakeRequestQueryStringAccessor : IRequestQueryStringAccessor
{
    public IQueryCollection Query { get; private set; } = new QueryCollection();

    public void SetFromText(string queryString)
    {
        Query = new QueryCollection(QueryHelpers.ParseQuery(queryString));
    }
}
