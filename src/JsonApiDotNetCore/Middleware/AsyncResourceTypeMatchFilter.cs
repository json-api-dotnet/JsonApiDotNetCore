using System;
using System.Collections;
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
        private readonly IJsonApiRequest _jsonApiRequest;

        public AsyncResourceTypeMatchFilter(IResourceContextProvider provider, IJsonApiRequest jsonApiRequest)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _jsonApiRequest = jsonApiRequest ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <inheritdoc />
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (next == null) throw new ArgumentNullException(nameof(next));

            if (context.HttpContext.IsJsonApiRequest() && IsPatchOrPostRequest(context.HttpContext.Request))
            {
                var resourceTypeFromEndpoint = GetEndpointResourceType();
                var resourceTypeFromBody = GetBodyResourceType(context);
                
                if (resourceTypeFromBody != null && resourceTypeFromEndpoint != null && resourceTypeFromBody != resourceTypeFromEndpoint)
                {
                    var resourceFromEndpoint = _provider.GetResourceContext(resourceTypeFromEndpoint);
                    var resourceFromBody = _provider.GetResourceContext(resourceTypeFromBody);

                    throw new ResourceTypeMismatchException(new HttpMethod(context.HttpContext.Request.Method), context.HttpContext.Request.Path,
                        resourceFromEndpoint, resourceFromBody);
                }
            }

            await next();
        }

        private bool IsPatchOrPostRequest(HttpRequest request)
        {
            return request.Method == HttpMethods.Patch || request.Method == HttpMethods.Post;
        }

        private Type GetBodyResourceType(ActionExecutingContext context)
        {
            var deserializedValue = context.ActionArguments.LastOrDefault().Value;
            if (deserializedValue is IList resourceCollection && resourceCollection.Count > 0)
            {
                return resourceCollection[0].GetType();
            }

            return deserializedValue?.GetType();
        }

        private Type GetEndpointResourceType()
        {
            if (_jsonApiRequest.Kind == EndpointKind.Primary)
            {
                return _jsonApiRequest.PrimaryResource.ResourceType;
            }
            
            return _jsonApiRequest.SecondaryResource?.ResourceType;
        }
    }
}
