using System.Reflection;
using System.Threading.Tasks;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.QueryStrings;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JsonApiDotNetCore.Middleware
{
    public sealed class QueryStringActionFilter : IQueryStringActionFilter
    {
        private readonly IQueryStringReader _queryStringReader;
    
        public QueryStringActionFilter(IQueryStringReader queryStringReader)
        {
            _queryStringReader = queryStringReader;
        }
        
        public void OnActionExecuted(ActionExecutedContext context) {  /* noop */ }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.IsJsonApiRequest())
            {
                return;
            }
            
            var disableQueryAttribute = context.Controller.GetType().GetCustomAttribute<DisableQueryAttribute>();
    
            _queryStringReader.ReadAll(disableQueryAttribute);
        }
    }
}
