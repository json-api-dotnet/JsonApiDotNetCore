using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
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
    public sealed class CurrentRequestMiddleware
    {
        private readonly RequestDelegate _next;
        private HttpContext _httpContext;
        private IJsonApiOptions _options;
        private ICurrentRequest _currentRequest;
        private IResourceGraph _resourceGraph;
        private RouteValueDictionary _routeValues;

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
            _httpContext = httpContext;
            _options = options;
            _currentRequest = currentRequest;
            _resourceGraph = resourceGraph;
            _routeValues = httpContext.GetRouteData().Values;

            var resourceContext = CreateResourceContext(controllerResourceMapping);
            if (resourceContext != null)
            {
                if (!await ValidateContentTypeHeaderAsync() || !await ValidateAcceptHeaderAsync())
                {
                    return;
                }

                SetupCurrentRequest(resourceContext);

                _httpContext.SetJsonApiRequest();
            }

            await _next(httpContext);
        }

        private ResourceContext CreateResourceContext(IControllerResourceMapping controllerResourceMapping)
        {
            var controllerName = (string)_routeValues["controller"];
            if (controllerName == null)
            {
                return null;
            }

            var resourceType = controllerResourceMapping.GetAssociatedResource(controllerName);
            var resourceContext = _resourceGraph.GetResourceContext(resourceType);
            return resourceContext;
        }

        private async Task<bool> ValidateContentTypeHeaderAsync()
        {
            var contentType = _httpContext.Request.ContentType;
            if (contentType != null)
            {
                if (!MediaTypeHeaderValue.TryParse(contentType, out var headerValue) ||
                    headerValue.MediaType != HeaderConstants.MediaType || headerValue.CharSet != null ||
                    headerValue.Parameters.Any(p => p.Name != "ext"))
                {
                    await FlushResponseAsync(_httpContext, new Error(HttpStatusCode.UnsupportedMediaType)
                    {
                        Title = "The specified Content-Type header value is not supported.",
                        Detail = $"Please specify '{HeaderConstants.MediaType}' instead of '{contentType}' for the Content-Type header value."
                    });

                    return false;
                }
            }

            return true;
        }

        private async Task<bool> ValidateAcceptHeaderAsync()
        {
            if (_httpContext.Request.Headers.TryGetValue("Accept", out StringValues acceptHeaders))
            {
                foreach (var acceptHeader in acceptHeaders)
                {
                    if (MediaTypeHeaderValue.TryParse(acceptHeader, out var headerValue))
                    {
                        if (headerValue.MediaType == HeaderConstants.MediaType &&
                            headerValue.Parameters.All(p => p.Name == "ext"))
                        {
                            return true;
                        }
                    }
                }

                await FlushResponseAsync(_httpContext, new Error(HttpStatusCode.NotAcceptable)
                {
                    Title = "The specified Accept header value is not supported.",
                    Detail = $"Please include '{HeaderConstants.MediaType}' in the Accept header values."
                });
                return false;
            }

            return true;
        }

        private async Task FlushResponseAsync(HttpContext context, Error error)
        {
            context.Response.StatusCode = (int) error.StatusCode;

            JsonSerializer serializer = JsonSerializer.CreateDefault(_options.SerializerSettings);
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
                await stream.CopyToAsync(context.Response.Body);
            }

            context.Response.Body.Flush();
        }

        private void SetupCurrentRequest(ResourceContext resourceContext)
        {
            _currentRequest.SetRequestResource(resourceContext);
            _currentRequest.BaseId = GetBaseId();
            _currentRequest.BasePath = GetBasePath(resourceContext.ResourceName);
            _currentRequest.IsRelationshipPath = GetIsRelationshipPath();
            _currentRequest.RelationshipId = GetRelationshipId();

            if (_routeValues.TryGetValue("relationshipName", out object relationshipName))
            {
                _currentRequest.RequestRelationship = resourceContext.Relationships.SingleOrDefault(r => r.PublicRelationshipName == (string) relationshipName);
            }
        }

        private string GetBaseId()
        {
            return _routeValues.TryGetValue("id", out var id) ? (string) id : null;
        }

        private string GetBasePath(string resourceName)
        {
            if (_options.RelativeLinks)
            {
                return _options.Namespace;
            }

            var customRoute = GetCustomRoute(_httpContext.Request.Path.Value, resourceName);
            var toReturn = $"{_httpContext.Request.Scheme}://{_httpContext.Request.Host}/{_options.Namespace}";
            if (customRoute != null)
            {
                toReturn += $"/{customRoute}";
            }
            return toReturn;
        }

        private string GetCustomRoute(string path, string resourceName)
        {
            var trimmedComponents = path.Trim('/').Split('/').ToList();
            var resourceNameIndex = trimmedComponents.FindIndex(c => c == resourceName);
            var newComponents = trimmedComponents.Take(resourceNameIndex).ToArray();
            var customRoute = string.Join('/', newComponents);
            return customRoute == _options.Namespace ? null : customRoute;
        }

        private bool GetIsRelationshipPath()
        {
            var actionName = (string)_routeValues["action"];
            return actionName.ToLowerInvariant().Contains("relationships");
        }

        private string GetRelationshipId()
        {
            if (!_currentRequest.IsRelationshipPath)
            {
                return null;
            }

            var components = SplitCurrentPath();
            return components.ElementAtOrDefault(4);
        }

        private string[] SplitCurrentPath()
        {
            var path = _httpContext.Request.Path.Value;
            var ns = $"/{_options.Namespace}";
            var nonNameSpaced = path.Replace(ns, "");
            nonNameSpaced = nonNameSpaced.Trim('/');
            return nonNameSpaced.Split('/');
        }
    }
}
