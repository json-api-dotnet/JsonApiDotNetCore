using JsonApiDotNetCore.QueryStrings;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCoreTests.UnitTests.Serialization.Response;

internal sealed class FakeRequestQueryStringAccessor : IRequestQueryStringAccessor
{
    public IQueryCollection Query { get; } = new QueryCollection(0);
}
