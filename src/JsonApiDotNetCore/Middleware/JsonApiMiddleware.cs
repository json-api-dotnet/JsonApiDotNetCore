using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models.JsonApiDocuments;
using Microsoft.AspNetCore.Http;
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
            IResourceGraph resourceGraph)
        {
            var routeValues = httpContext.GetRouteData().Values;

            var resourceContext = CreateResourceContext(routeValues, controllerResourceMapping, resourceGraph);
            if (resourceContext != null)
            {
                if (!await ValidateContentTypeHeaderAsync(httpContext, options.SerializerSettings) || 
                    !await ValidateAcceptHeaderAsync(httpContext, options.SerializerSettings))
                {
                    return;
                }

                SetupCurrentRequest(currentRequest, resourceContext, routeValues, options, httpContext.Request);

                httpContext.SetJsonApiRequest();
            }

            await _next(httpContext);
        }

        private static ResourceContext CreateResourceContext(RouteValueDictionary routeValues,
            IControllerResourceMapping controllerResourceMapping, IResourceContextProvider resourceGraph)
        {
            var controllerName = (string) routeValues["controller"];
            if (controllerName == null)
            {
                return null;
            }

            var resourceType = controllerResourceMapping.GetAssociatedResource(controllerName);
            return resourceGraph.GetResourceContext(resourceType);
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

            httpResponse.Body.Flush();
        }

        private static void SetupCurrentRequest(ICurrentRequest currentRequest, ResourceContext resourceContext,
            RouteValueDictionary routeValues, IJsonApiOptions options, HttpRequest httpRequest)
        {
            currentRequest.SetRequestResource(resourceContext);
            currentRequest.BaseId = GetBaseId(routeValues);
            currentRequest.BasePath = GetBasePath(resourceContext.ResourceName, options, httpRequest);
            currentRequest.IsRelationshipPath = GetIsRelationshipPath(routeValues);
            currentRequest.RelationshipId = GetRelationshipId(currentRequest.IsRelationshipPath, httpRequest.Path.Value, options.Namespace);

            if (routeValues.TryGetValue("relationshipName", out object relationshipName))
            {
                currentRequest.RequestRelationship =
                    resourceContext.Relationships.SingleOrDefault(relationship =>
                        relationship.PublicRelationshipName == (string) relationshipName);
            }
        }

        private static string GetBaseId(RouteValueDictionary routeValues)
        {
            return routeValues.TryGetValue("id", out var id) ? (string) id : null;
        }

        private static string GetBasePath(string resourceName, IJsonApiOptions options, HttpRequest httpRequest)
        {
            var builder = new StringBuilder();

            if (!options.RelativeLinks)
            {
                builder.Append(httpRequest.Scheme);
                builder.Append("://");
                builder.Append(httpRequest.Host);
            }

            string customRoute = GetCustomRoute(httpRequest.Path.Value, resourceName, options.Namespace);
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

        private static string GetCustomRoute(string path, string resourceName, string apiNamespace)
        {
            var trimmedComponents = path.Trim('/').Split('/').ToList();
            var resourceNameIndex = trimmedComponents.FindIndex(c => c == resourceName);
            var newComponents = trimmedComponents.Take(resourceNameIndex).ToArray();
            var customRoute = string.Join('/', newComponents);
            return customRoute == apiNamespace ? null : customRoute;
        }

        private static bool GetIsRelationshipPath(RouteValueDictionary routeValues)
        {
            var actionName = (string)routeValues["action"];
            return actionName.ToLowerInvariant().Contains("relationships");
        }

        private static string GetRelationshipId(bool currentRequestIsRelationshipPath, string requestPath,
            string apiNamespace)
        {
            if (!currentRequestIsRelationshipPath)
            {
                return null;
            }

            var components = SplitCurrentPath(requestPath, apiNamespace);
            return components.ElementAtOrDefault(4);
        }

        private static IEnumerable<string> SplitCurrentPath(string requestPath, string apiNamespace)
        {
            var namespacePrefix = $"/{apiNamespace}";
            var nonNameSpaced = requestPath.Replace(namespacePrefix, "");
            nonNameSpaced = nonNameSpaced.Trim('/');
            return nonNameSpaced.Split('/');
        }
    }
}
