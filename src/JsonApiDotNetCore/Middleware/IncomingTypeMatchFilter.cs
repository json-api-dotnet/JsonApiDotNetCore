using System;
using System.Collections;
using System.Linq;
using System.Net.Http;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JsonApiDotNetCore.Middleware
{
    /// <summary>
    /// Action filter used to verify the incoming resource type matches the target type, else return a 409.
    /// </summary>
    public sealed class IncomingTypeMatchFilter : IActionFilter
    {
        private readonly IResourceContextProvider _provider;
        private readonly IJsonApiRequest _jsonApiRequest;

        public IncomingTypeMatchFilter(IResourceContextProvider provider, IJsonApiRequest jsonApiRequest)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _jsonApiRequest = jsonApiRequest ?? throw new ArgumentNullException(nameof(provider));
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (!context.HttpContext.IsJsonApiRequest())
            {
                return;
            }

            var request = context.HttpContext.Request;
            if (request.Method == HttpMethods.Patch || request.Method == HttpMethods.Post)
            {
                var deserializedType = GetDeserializedType(context);
                var targetType = GetTargetType();
                
                if (deserializedType != null && targetType != null && deserializedType != targetType)
                {
                    ResourceContext resourceFromEndpoint = _provider.GetResourceContext(targetType);
                    ResourceContext resourceFromBody = _provider.GetResourceContext(deserializedType);

                    throw new ResourceTypeMismatchException(new HttpMethod(request.Method), request.Path, resourceFromEndpoint, resourceFromBody);
                }
            }
        }

        private Type GetDeserializedType(ActionExecutingContext context)
        {
            var deserializedValue = context.ActionArguments.LastOrDefault().Value;
            if (deserializedValue is IList resourceCollection && resourceCollection.Count > 0)
            {
                return resourceCollection[0].GetType();
            }

            return deserializedValue?.GetType();
        }

        private Type GetTargetType()
        {
            if (_jsonApiRequest.Kind == EndpointKind.Primary)
            {
                return _jsonApiRequest.PrimaryResource.ResourceType;
            }
            
            return _jsonApiRequest.SecondaryResource?.ResourceType;
        }

        public void OnActionExecuted(ActionExecutedContext context) { /* noop */ }
    }
}
