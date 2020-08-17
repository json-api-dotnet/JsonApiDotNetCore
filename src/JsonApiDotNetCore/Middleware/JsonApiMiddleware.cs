using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models.Annotation;
using JsonApiDotNetCore.Models.JsonApiDocuments;
using JsonApiDotNetCore.RequestServices;
using JsonApiDotNetCore.RequestServices.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Middleware
{
    /// <summary>
    /// Intercepts HTTP requests to populate injected <see cref="ICurrentRequest"/> instance for json:api requests.
    /// </summary>
    public sealed class JsonApiMiddleware
    {
        private readonly RequestDelegate _next;

        public JsonApiMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext, 
            IControllerResourceMapping controllerResourceMapping, 
            IJsonApiOptions options, 
            ICurrentRequest currentRequest, 
            IResourceContextProvider resourceContextProvider)
        {
            var routeValues = httpContext.GetRouteData().Values;

            var primaryResourceContext = CreatePrimaryResourceContext(routeValues, controllerResourceMapping, resourceContextProvider);
            if (primaryResourceContext != null)
            {
                if (!await ValidateContentTypeHeaderAsync(httpContext, options.SerializerSettings) || 
                    !await ValidateAcceptHeaderAsync(httpContext, options.SerializerSettings))
                {
                    return;
                }

                SetupCurrentRequest((CurrentRequest)currentRequest, primaryResourceContext, routeValues, options, resourceContextProvider, httpContext.Request);

                httpContext.SetJsonApiRequest();
            }

            await _next(httpContext);
        }

        private static ResourceContext CreatePrimaryResourceContext(RouteValueDictionary routeValues,
            IControllerResourceMapping controllerResourceMapping, IResourceContextProvider resourceContextProvider)
        {
            var controllerName = (string) routeValues["controller"];
            if (controllerName == null)
            {
                return null;
            }

            var resourceType = controllerResourceMapping.GetAssociatedResource(controllerName);
            return resourceContextProvider.GetResourceContext(resourceType);
        }

        private static async Task<bool> ValidateContentTypeHeaderAsync(HttpContext httpContext, JsonSerializerSettings serializerSettings)
        {
            var contentType = httpContext.Request.ContentType;
            if (contentType != null && contentType != HeaderConstants.MediaType)
            {
                await FlushResponseAsync(httpContext.Response, serializerSettings, new Error(HttpStatusCode.UnsupportedMediaType)
                {
                    Title = "The specified Content-Type header value is not supported.",
                    Detail = $"Please specify '{HeaderConstants.MediaType}' instead of '{contentType}' for the Content-Type header value."
                });
                return false;
            }

            return true;
        }

        private static async Task<bool> ValidateAcceptHeaderAsync(HttpContext httpContext, JsonSerializerSettings serializerSettings)
        {
            StringValues acceptHeaders = httpContext.Request.Headers["Accept"];
            if (!acceptHeaders.Any() || acceptHeaders == HeaderConstants.MediaType)
            {
                return true;
            }

            bool seenCompatibleMediaType = false;

            foreach (var acceptHeader in acceptHeaders)
            {
                if (MediaTypeHeaderValue.TryParse(acceptHeader, out var headerValue))
                {
                    if (headerValue.MediaType == "*/*" || headerValue.MediaType == "application/*")
                    {
                        seenCompatibleMediaType = true;
                        break;
                    }

                    if (headerValue.MediaType == HeaderConstants.MediaType && !headerValue.Parameters.Any())
                    {
                        seenCompatibleMediaType = true;
                        break;
                    }
                }
            }

            if (!seenCompatibleMediaType)
            {
                await FlushResponseAsync(httpContext.Response, serializerSettings, new Error(HttpStatusCode.NotAcceptable)
                {
                    Title = "The specified Accept header value does not contain any supported media types.",
                    Detail = $"Please include '{HeaderConstants.MediaType}' in the Accept header values."
                });
                return false;
            }

            return true;
        }

        private static async Task FlushResponseAsync(HttpResponse httpResponse, JsonSerializerSettings serializerSettings, Error error)
        {
            httpResponse.StatusCode = (int) error.StatusCode;

            JsonSerializer serializer = JsonSerializer.CreateDefault(serializerSettings);
            serializer.ApplyErrorSettings();

            // https://github.com/JamesNK/Newtonsoft.Json/issues/1193
            await using (var stream = new MemoryStream())
            {
                await using (var streamWriter = new StreamWriter(stream, leaveOpen: true))
                {
                    using var jsonWriter = new JsonTextWriter(streamWriter);
                    serializer.Serialize(jsonWriter, new ErrorDocument(error));
                }

                stream.Seek(0, SeekOrigin.Begin);
                await stream.CopyToAsync(httpResponse.Body);
            }

            await httpResponse.Body.FlushAsync();
        }

        private static void SetupCurrentRequest(CurrentRequest currentRequest, ResourceContext primaryResourceContext,
            RouteValueDictionary routeValues, IJsonApiOptions options, IResourceContextProvider resourceContextProvider,
            HttpRequest httpRequest)
        {
            currentRequest.IsReadOnly = httpRequest.Method == HttpMethod.Get.Method;
            currentRequest.Kind = EndpointKind.Primary;
            currentRequest.PrimaryResource = primaryResourceContext;
            currentRequest.PrimaryId = GetPrimaryRequestId(routeValues);
            currentRequest.BasePath = GetBasePath(primaryResourceContext.ResourceName, options, httpRequest);

            var relationshipName = GetRelationshipNameForSecondaryRequest(routeValues);
            if (relationshipName != null)
            {
                currentRequest.Kind = IsRouteForRelationship(routeValues) ? EndpointKind.Relationship : EndpointKind.Secondary;

                var requestRelationship =
                    primaryResourceContext.Relationships.SingleOrDefault(relationship =>
                        relationship.PublicName == relationshipName);

                if (requestRelationship != null)
                {
                    currentRequest.Relationship = requestRelationship;
                    currentRequest.SecondaryResource = resourceContextProvider.GetResourceContext(requestRelationship.RightType);
                }
            }

            currentRequest.IsCollection = currentRequest.PrimaryId == null || currentRequest.Relationship is HasManyAttribute;
        }

        private static string GetPrimaryRequestId(RouteValueDictionary routeValues)
        {
            return routeValues.TryGetValue("id", out var id) ? (string) id : null;
        }

        private static string GetBasePath(string resourceName, IJsonApiOptions options, HttpRequest httpRequest)
        {
            var builder = new StringBuilder();

            if (!options.UseRelativeLinks)
            {
                builder.Append(httpRequest.Scheme);
                builder.Append("://");
                builder.Append(httpRequest.Host);
            }

            string customRoute = GetCustomRoute(resourceName, options.Namespace, httpRequest.HttpContext);
            if (!string.IsNullOrEmpty(customRoute))
            {
                builder.Append('/');
                builder.Append(customRoute);
            }
            else if (!string.IsNullOrEmpty(options.Namespace))
            {
                builder.Append('/');
                builder.Append(options.Namespace);
            }

            return builder.ToString();
        }

        private static string GetCustomRoute(string resourceName, string apiNamespace, HttpContext httpContext)
        {
            var endpoint = httpContext.GetEndpoint();
            var routeAttribute = endpoint.Metadata.GetMetadata<RouteAttribute>();
            if (routeAttribute != null)
            {
                var trimmedComponents = httpContext.Request.Path.Value.Trim('/').Split('/').ToList();
                var resourceNameIndex = trimmedComponents.FindIndex(c => c == resourceName);
                var newComponents = trimmedComponents.Take(resourceNameIndex).ToArray();
                var customRoute = string.Join('/', newComponents);
                return customRoute == apiNamespace ? null : customRoute;
            }

            return null;
        }

        private static string GetRelationshipNameForSecondaryRequest(RouteValueDictionary routeValues)
        {
            return routeValues.TryGetValue("relationshipName", out object routeValue) ? (string) routeValue : null;
        }

        private static bool IsRouteForRelationship(RouteValueDictionary routeValues)
        {
            var actionName = (string)routeValues["action"];
            return actionName.EndsWith("Relationship");
        }
    }
}
