using System;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JsonApiDotNetCore.Middleware
{
    /// <summary>
    /// Action filter used to verify the incoming type matches the target type, else return a 409
    /// </summary>
    public class DefaultTypeMatchFilter : IActionFilter
    {
        private readonly IContextEntityProvider _provider;

        public DefaultTypeMatchFilter(IContextEntityProvider provider)
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
                    var expectedJsonApiResource = _provider.GetContextEntity(targetType);

                    throw new JsonApiException(409,
                        $"Cannot '{context.HttpContext.Request.Method}' type '{deserializedType.Name}' "
                        + $"to '{expectedJsonApiResource?.EntityName}' endpoint.",
                        detail: "Check that the request payload type matches the type expected by this endpoint.");
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
