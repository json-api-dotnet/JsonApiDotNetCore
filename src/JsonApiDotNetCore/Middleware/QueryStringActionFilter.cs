using System.Reflection;
using System.Threading.Tasks;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Internal.QueryStrings;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JsonApiDotNetCore.Middleware
{
    public sealed class QueryStringActionFilter : IAsyncActionFilter, IQueryStringActionFilter
    {
        private readonly IQueryStringReader _queryStringReader;

        public QueryStringActionFilter(IQueryStringReader queryStringReader)
        {
            _queryStringReader = queryStringReader;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            DisableQueryAttribute disableQueryAttribute = context.Controller.GetType().GetCustomAttribute<DisableQueryAttribute>();

            _queryStringReader.ReadAll(disableQueryAttribute);
            await next();
        }
    }
}
