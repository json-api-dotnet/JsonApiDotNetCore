using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using JsonApiDotNetCore.Exceptions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JsonApiDotNetCore.Middleware
{
    /// <summary>
    /// Action filter used to verify the incoming type matches the target type, else return a 409
    /// </summary>
    public sealed class DefaultTypeMatchFilter : IActionFilter
    {
        private readonly IResourceContextProvider _provider;

        public DefaultTypeMatchFilter(IResourceContextProvider provider)
        {
            _provider = provider;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var request = context.HttpContext.Request;
            if (IsJsonApiRequest(request) && (request.Method == "PATCH" || request.Method == "POST"))
            {
                var deserializedType = context.ActionArguments.FirstOrDefault().Value?.GetType();
                var targetType = context.ActionDescriptor.Parameters.FirstOrDefault()?.ParameterType;

                if (deserializedType != null && targetType != null && deserializedType != targetType)
                {
                    ResourceContext resourceFromEndpoint = _provider.GetResourceContext(targetType);
                    ResourceContext resourceFromBody = _provider.GetResourceContext(deserializedType);

                    throw new ResourceTypeMismatchException(new HttpMethod(request.Method), request.Path, resourceFromEndpoint, resourceFromBody);
                }
            }
        }

        private bool IsJsonApiRequest(HttpRequest request)
        {
            return (request.ContentType?.Equals(Constants.ContentType, StringComparison.OrdinalIgnoreCase) == true);
        }

        public void OnActionExecuted(ActionExecutedContext context) { /* noop */ }
    }
}
