using System.Reflection;
using System.Threading.Tasks;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JsonApiDotNetCore.Middleware
{
    public class QueryParameterActionFilter : IAsyncActionFilter, IQueryParameterActionFilter
    {
        private readonly IQueryParameterParser _queryParser;
        public QueryParameterActionFilter(IQueryParameterParser queryParser) => _queryParser = queryParser;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // gets the DisableQueryAttribute if set on the controller that is targeted by the current request.
            DisableQueryAttribute disabledQuery = context.Controller.GetType().GetTypeInfo().GetCustomAttribute(typeof(DisableQueryAttribute)) as DisableQueryAttribute;

            _queryParser.Parse(context.HttpContext.Request.Query, disabledQuery);
            await next();
        }
    }
}
