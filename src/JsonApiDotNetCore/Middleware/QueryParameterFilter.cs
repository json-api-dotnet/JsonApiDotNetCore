using System.Reflection;
using System.Threading.Tasks;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JsonApiDotNetCore.Middleware
{
    public sealed class QueryParameterActionFilter : IAsyncActionFilter, IQueryParameterActionFilter
    {
        private readonly IQueryParameterParser _queryParser;
        public QueryParameterActionFilter(IQueryParameterParser queryParser) => _queryParser = queryParser;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            DisableQueryAttribute disableQueryAttribute = context.Controller.GetType().GetCustomAttribute<DisableQueryAttribute>();

            _queryParser.Parse(disableQueryAttribute);
            await next();
        }
    }
}
