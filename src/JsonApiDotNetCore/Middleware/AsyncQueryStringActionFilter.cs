using System;
using System.Reflection;
using System.Threading.Tasks;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.QueryStrings;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JsonApiDotNetCore.Middleware
{
    /// <inheritdoc />
    public sealed class AsyncQueryStringActionFilter : IAsyncQueryStringActionFilter
    {
        private readonly IQueryStringReader _queryStringReader;
    
        public AsyncQueryStringActionFilter(IQueryStringReader queryStringReader)
        {
            _queryStringReader = queryStringReader ?? throw new ArgumentNullException(nameof(queryStringReader));
        }

        /// <inheritdoc />
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (next == null) throw new ArgumentNullException(nameof(next));

            if (context.HttpContext.IsJsonApiRequest())
            {
                var disableQueryStringAttribute = context.Controller.GetType().GetCustomAttribute<DisableQueryStringAttribute>();
                _queryStringReader.ReadAll(disableQueryStringAttribute);
            }

            await next();
        }
    }
}
