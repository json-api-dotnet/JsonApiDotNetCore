using System.Reflection;
using System.Threading.Tasks;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JsonApiDotNetCore.Middleware
{
    public class QueryParameterActionFilter : IAsyncActionFilter, IQueryParameterActionFilter
    {
        private readonly IQueryParser _queryParser;
        public QueryParameterActionFilter(IQueryParser queryParser) => _queryParser = queryParser;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            DisableQueryAttribute disabledQuery = context.Controller.GetType().GetTypeInfo().GetCustomAttribute(typeof(DisableQueryAttribute)) as DisableQueryAttribute;
            _queryParser.Parse(context.HttpContext.Request.Query, disabledQuery);
            await next();
        }
    }
}
