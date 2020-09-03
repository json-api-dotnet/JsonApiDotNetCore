using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JsonApiDotNetCore.Middleware
{
    /// <inheritdoc />
    public sealed class AsyncResourceTypeMatchFilter : IAsyncResourceTypeMatchFilter
    {
        private readonly IResourceContextProvider _provider;

        public AsyncResourceTypeMatchFilter(IResourceContextProvider provider)
        {
            _provider = provider;
        }

        /// <inheritdoc />
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (next == null) throw new ArgumentNullException(nameof(next));

            if (context.HttpContext.IsJsonApiRequest() && IsPatchOrPostRequest(context.HttpContext.Request))
            {
                var deserializedType = context.ActionArguments.FirstOrDefault().Value?.GetType();
                var targetType = context.ActionDescriptor.Parameters.FirstOrDefault()?.ParameterType;

                if (deserializedType != null && targetType != null && deserializedType != targetType)
                {
                    var resourceFromEndpoint = _provider.GetResourceContext(targetType);
                    var resourceFromBody = _provider.GetResourceContext(deserializedType);

                    throw new ResourceTypeMismatchException(new HttpMethod(context.HttpContext.Request.Method), context.HttpContext.Request.Path,
                        resourceFromEndpoint, resourceFromBody);
                }
            }

            await next();
        }

        private static bool IsPatchOrPostRequest(HttpRequest request)
        {
            return request.Method == "PATCH" || request.Method == "POST";
        }
    }
}
