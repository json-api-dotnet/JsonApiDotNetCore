using System;
using System.Reflection;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.QueryStrings;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JsonApiDotNetCore.Middleware
{
    public sealed class QueryStringActionFilter : IQueryStringActionFilter
    {
        private readonly IQueryStringReader _queryStringReader;
    
        public QueryStringActionFilter(IQueryStringReader queryStringReader)
        {
            _queryStringReader = queryStringReader ?? throw new ArgumentNullException(nameof(queryStringReader));
        }
        
        public void OnActionExecuted(ActionExecutedContext context) {  /* noop */ }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            
            if (!context.HttpContext.IsJsonApiRequest())
            {
                return;
            }
            
            var disableQueryStringAttribute = context.Controller.GetType().GetCustomAttribute<DisableQueryStringAttribute>();
            _queryStringReader.ReadAll(disableQueryStringAttribute);
        }
    }
}
