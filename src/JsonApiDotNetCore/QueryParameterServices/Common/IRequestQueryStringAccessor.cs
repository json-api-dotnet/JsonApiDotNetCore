using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.QueryParameterServices.Common
{
    public interface IRequestQueryStringAccessor
    {
        QueryString QueryString { get; }
        IQueryCollection Query { get; }
    }
}
