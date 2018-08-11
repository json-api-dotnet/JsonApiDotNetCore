using System;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JsonApiDotNetCore.Middleware
{
    public class TypeMatchFilter : IActionFilter
    {
        private readonly IJsonApiContext _jsonApiContext;

        public TypeMatchFilter(IJsonApiContext jsonApiContext)
        {
            _jsonApiContext = jsonApiContext;
        }

        /// <summary>
        /// Used to verify the incoming type matches the target type, else return a 409
        /// </summary>
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var request = context.HttpContext.Request;
            if (IsJsonApiRequest(request) && request.Method == "PATCH" || request.Method == "POST")
            {
                var deserializedType = context.ActionArguments.FirstOrDefault().Value?.GetType();
                var targetType = context.ActionDescriptor.Parameters.FirstOrDefault()?.ParameterType;

                if (deserializedType != null && targetType != null && deserializedType != targetType)
                {
                    var expectedJsonApiResource = _jsonApiContext.ContextGraph.GetContextEntity(targetType);

                    throw new JsonApiException(409,
                        $"Cannot '{context.HttpContext.Request.Method}' type '{_jsonApiContext.RequestEntity.EntityName}' "
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
