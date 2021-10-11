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
            ArgumentGuard.NotNull(queryStringReader, nameof(queryStringReader));

            _queryStringReader = queryStringReader;
        }

        /// <inheritdoc />
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            ArgumentGuard.NotNull(context, nameof(context));
            ArgumentGuard.NotNull(next, nameof(next));

            if (context.HttpContext.IsJsonApiRequest())
            {
                var disableQueryStringAttribute = context.Controller.GetType().GetCustomAttribute<DisableQueryStringAttribute>();
                _queryStringReader.ReadAll(disableQueryStringAttribute);
            }

            await next();
        }
    }
}
