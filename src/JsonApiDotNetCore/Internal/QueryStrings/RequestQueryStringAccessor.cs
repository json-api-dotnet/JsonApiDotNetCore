using JsonApiDotNetCore.QueryStrings;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Internal.QueryStrings
{
    /// <inheritdoc/>
    internal sealed class RequestQueryStringAccessor : IRequestQueryStringAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public QueryString QueryString => _httpContextAccessor.HttpContext.Request.QueryString;
        public IQueryCollection Query => _httpContextAccessor.HttpContext.Request.Query;

        public RequestQueryStringAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
    }
}
