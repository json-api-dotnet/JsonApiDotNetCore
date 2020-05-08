using System;
using System.IO;
using System.Linq;
using System.Net;
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
    /// This sets all necessary parameters relating to the HttpContext for JADNC
    /// </summary>
    public sealed class CurrentRequestMiddleware
    {
        private readonly RequestDelegate _next;

        public CurrentRequestMiddleware(RequestDelegate next)
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
            var requestContext = new RequestContext(httpContext, currentRequest, resourceGraph, options, routeValues,
                controllerResourceMapping);

            var requestResource = GetCurrentEntity(requestContext);
            if (requestResource != null)
            {
                requestContext.CurrentRequest.SetRequestResource(requestResource);
                requestContext.CurrentRequest.IsRelationshipPath = PathIsRelationship(requestContext.RouteValues);
                requestContext.CurrentRequest.BasePath = GetBasePath(requestContext, requestResource.ResourceName);
                requestContext.CurrentRequest.BaseId = GetBaseId(requestContext.RouteValues);
                requestContext.CurrentRequest.RelationshipId = GetRelationshipId(requestContext);
            }

            if (await IsValidAsync(requestContext))
            {
                await _next(requestContext.HttpContext);
            }
        }

        private static string GetBaseId(RouteValueDictionary routeValues)
        {
            if (routeValues.TryGetValue("id", out object stringId))
            {
                return (string)stringId;
            }

            return null;
        }

        private static string GetRelationshipId(RequestContext requestContext)
        {
            if (!requestContext.CurrentRequest.IsRelationshipPath)
            {
                return null;
            }
            var components = SplitCurrentPath(requestContext);
            var toReturn = components.ElementAtOrDefault(4);

            return toReturn;
        }

        private static string[] SplitCurrentPath(RequestContext requestContext)
        {
            var path = requestContext.HttpContext.Request.Path.Value;
            var ns = $"/{requestContext.Options.Namespace}";
            var nonNameSpaced = path.Replace(ns, "");
            nonNameSpaced = nonNameSpaced.Trim('/');
            var individualComponents = nonNameSpaced.Split('/');
            return individualComponents;
        }

        private static string GetBasePath(RequestContext requestContext, string resourceName = null)
        {
            var r = requestContext.HttpContext.Request;
            if (requestContext.Options.RelativeLinks)
            {
                return requestContext.Options.Namespace;
            }

            var customRoute = GetCustomRoute(requestContext.Options, r.Path.Value, resourceName);
            var toReturn = $"{r.Scheme}://{r.Host}/{requestContext.Options.Namespace}";
            if (customRoute != null)
            {
                toReturn += $"/{customRoute}";
            }
            return toReturn;
        }

        private static object GetCustomRoute(IJsonApiOptions options, string path, string resourceName)
        {
            var trimmedComponents = path.Trim('/').Split('/').ToList();
            var resourceNameIndex = trimmedComponents.FindIndex(c => c == resourceName);
            var newComponents = trimmedComponents.Take(resourceNameIndex).ToArray();
            var customRoute = string.Join('/', newComponents);
            if (customRoute == options.Namespace)
            {
                return null;
            }
            else
            {
                return customRoute;
            }
        }

        private static bool PathIsRelationship(RouteValueDictionary routeValues)
        {
            var actionName = (string)routeValues["action"];
            return actionName.ToLowerInvariant().Contains("relationships");
        }

        private static async Task<bool> IsValidAsync(RequestContext requestContext)
        {
            return await IsValidContentTypeHeaderAsync(requestContext) && await IsValidAcceptHeaderAsync(requestContext);
        }

        private static async Task<bool> IsValidContentTypeHeaderAsync(RequestContext requestContext)
        {
            var contentType = requestContext.HttpContext.Request.ContentType;
            if (contentType != null && ContainsMediaTypeParameters(contentType))
            {
                await FlushResponseAsync(requestContext, new Error(HttpStatusCode.UnsupportedMediaType)
                {
                    Title = "The specified Content-Type header value is not supported.",
                    Detail = $"Please specify '{HeaderConstants.ContentType}' for the Content-Type header value."
                });

                return false;
            }
            return true;
        }

        private static async Task<bool> IsValidAcceptHeaderAsync(RequestContext requestContext)
        {
            if (requestContext.HttpContext.Request.Headers.TryGetValue(HeaderConstants.AcceptHeader, out StringValues acceptHeaders) == false)
                return true;

            foreach (var acceptHeader in acceptHeaders)
            {
                if (ContainsMediaTypeParameters(acceptHeader) == false)
                {
                    continue;
                }

                await FlushResponseAsync(requestContext, new Error(HttpStatusCode.NotAcceptable)
                {
                    Title = "The specified Accept header value is not supported.",
                    Detail = $"Please specify '{HeaderConstants.ContentType}' for the Accept header value."
                });
                return false;
            }
            return true;
        }

        private static bool ContainsMediaTypeParameters(string mediaType)
        {
            var incomingMediaTypeSpan = mediaType.AsSpan();

            // if the content type is not application/vnd.api+json then continue on
            if (incomingMediaTypeSpan.Length < HeaderConstants.ContentType.Length)
            {
                return false;
            }

            var incomingContentType = incomingMediaTypeSpan.Slice(0, HeaderConstants.ContentType.Length);
            if (incomingContentType.SequenceEqual(HeaderConstants.ContentType.AsSpan()) == false)
                return false;

            // anything appended to "application/vnd.api+json;" will be considered a media type param
            return (
                incomingMediaTypeSpan.Length >= HeaderConstants.ContentType.Length + 2
                && incomingMediaTypeSpan[HeaderConstants.ContentType.Length] == ';'
            );
        }

        private static async Task FlushResponseAsync(RequestContext requestContext, Error error)
        {
            requestContext.HttpContext.Response.StatusCode = (int) error.StatusCode;

            JsonSerializer serializer = JsonSerializer.CreateDefault(requestContext.Options.SerializerSettings);
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
                await stream.CopyToAsync(requestContext.HttpContext.Response.Body);
            }

            requestContext.HttpContext.Response.Body.Flush();
        }

        /// <summary>
        /// Gets the current entity that we need for serialization and deserialization.
        /// </summary>
        /// <returns></returns>
        private static ResourceContext GetCurrentEntity(RequestContext requestContext)
        {
            var controllerName = (string)requestContext.RouteValues["controller"];
            if (controllerName == null)
            {
                return null;
            }
            var resourceType = requestContext.ControllerResourceMapping.GetAssociatedResource(controllerName);
            var requestResource = requestContext.ResourceGraph.GetResourceContext(resourceType);
            if (requestResource == null)
            {
                return null;
            }
            if (requestContext.RouteValues.TryGetValue("relationshipName", out object relationshipName))
            {
                requestContext.CurrentRequest.RequestRelationship = requestResource.Relationships.SingleOrDefault(r => r.PublicRelationshipName == (string)relationshipName);
            }
            return requestResource;
        }

        private sealed class RequestContext
        {
            public HttpContext HttpContext { get; }
            public ICurrentRequest CurrentRequest { get; }
            public IResourceGraph ResourceGraph { get; }
            public IJsonApiOptions Options { get; }
            public RouteValueDictionary RouteValues { get; }
            public IControllerResourceMapping ControllerResourceMapping { get; }

            public RequestContext(HttpContext httpContext, 
                ICurrentRequest currentRequest, 
                IResourceGraph resourceGraph,
                IJsonApiOptions options, 
                RouteValueDictionary routeValues,
                IControllerResourceMapping controllerResourceMapping)
            {
                HttpContext = httpContext;
                CurrentRequest = currentRequest;
                ResourceGraph = resourceGraph;
                Options = options;
                RouteValues = routeValues;
                ControllerResourceMapping = controllerResourceMapping;
            }
        }
    }
}
