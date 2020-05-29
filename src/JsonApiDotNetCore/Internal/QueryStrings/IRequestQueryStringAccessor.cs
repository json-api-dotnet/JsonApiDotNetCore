using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Internal.QueryStrings
{
    public interface IRequestQueryStringAccessor
    {
        QueryString QueryString { get; }
        IQueryCollection Query { get; }
    }
}
