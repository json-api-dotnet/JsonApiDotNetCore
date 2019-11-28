using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Managers.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.Middleware
{
    /// <summary>
    /// This sets all necessary parameters relating to the HttpContext for JADNC
    /// </summary>
    public class CurrentRequestMiddleware
    {
        private readonly RequestDelegate _next;
        private HttpContext _httpContext;
        private ICurrentRequest _currentRequest;
        private IResourceGraph _resourceGraph;
        private IJsonApiOptions _options;
        private RouteValueDictionary _routeValues;
        private IControllerResourceMapping _controllerResourceMapping;

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
            _currentRequest = currentRequest;
            _controllerResourceMapping = controllerResourceMapping;
            _resourceGraph = resourceGraph;
            _options = options;
            _routeValues = httpContext.GetRouteData().Values;
            var requestResource = GetCurrentEntity();
            if (requestResource != null)
            {
                _currentRequest.SetRequestResource(requestResource);
                _currentRequest.IsRelationshipPath = PathIsRelationship();
                _currentRequest.BasePath = GetBasePath(requestResource.ResourceName);
                _currentRequest.BaseId = GetBaseId();
                _currentRequest.RelationshipId = GetRelationshipId();
            }

            if (IsValid())
            {
                await _next(httpContext);
            }
        }

        private string GetBaseId()
        {
            var path = _httpContext.Request.Path.Value;
            var resource = _currentRequest.GetRequestResource();
            var resourceName = resource.ResourceName;
            var ns = $"/{GetNameSpace()}";
            var nonNameSpaced = path.Replace(ns, "");
            nonNameSpaced = nonNameSpaced.Trim('/');

            var individualComponents = nonNameSpaced.Split('/');
            if (individualComponents.Length < 2)
            {
                return null;
            }
            var toReturn = individualComponents[1];

            CheckIdType(toReturn, resource.IdentityType);




            return individualComponents[2];
        }
        private void CheckIdType(string value, Type idType)
        {

            try
            {
                var converter = TypeDescriptor.GetConverter(idType);
                if (converter != null)
                {
                    if (!converter.IsValid(value))
                    {
                        throw new JsonApiException(500, $"We could not convert the id '{value}'");
                    }
                    else
                    {
                        if (idType == typeof(int))
                        {
                            if ((int)converter.ConvertFromString(value) < 0)
                            {
                                throw new JsonApiException(500, "The base ID is an integer, and it is negative.");
                            }
                        }
                    }
                }
            }
            catch (NotSupportedException)
            {

            }

        }
        private string GetRelationshipId()
        {
            return "hello";
        }

        private string GetBasePath(string entityName)
        {
            var r = _httpContext.Request;
            if (_options.RelativeLinks)
            {
                return GetNameSpace();
            }
            var ns = GetNameSpace();
            return $"{r.Scheme}://{r.Host}/{ns}";
        }

        private string GetNameSpace()
        {
            return _options.Namespace;
        }

        protected bool PathIsRelationship()
        {
            var actionName = (string)_routeValues["action"];
            return actionName.ToLower().Contains("relationships");
        }

        private bool IsValid()
        {
            return IsValidContentTypeHeader(_httpContext) && IsValidAcceptHeader(_httpContext);
        }

        private bool IsValidContentTypeHeader(HttpContext context)
        {
            var contentType = context.Request.ContentType;
            if (contentType != null && ContainsMediaTypeParameters(contentType))
            {
                FlushResponse(context, 415);
                return false;
            }
            return true;
        }

        private bool IsValidAcceptHeader(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(Constants.AcceptHeader, out StringValues acceptHeaders) == false)
                return true;

            foreach (var acceptHeader in acceptHeaders)
            {
                if (ContainsMediaTypeParameters(acceptHeader) == false)
                {
                    continue;
                }

                FlushResponse(context, 406);
                return false;
            }
            return true;
        }

        internal static bool ContainsMediaTypeParameters(string mediaType)
        {
            var incomingMediaTypeSpan = mediaType.AsSpan();

            // if the content type is not application/vnd.api+json then continue on
            if (incomingMediaTypeSpan.Length < Constants.ContentType.Length)
            {
                return false;
            }

            var incomingContentType = incomingMediaTypeSpan.Slice(0, Constants.ContentType.Length);
            if (incomingContentType.SequenceEqual(Constants.ContentType.AsSpan()) == false)
                return false;

            // anything appended to "application/vnd.api+json;" will be considered a media type param
            return (
                incomingMediaTypeSpan.Length >= Constants.ContentType.Length + 2
                && incomingMediaTypeSpan[Constants.ContentType.Length] == ';'
            );
        }

        private void FlushResponse(HttpContext context, int statusCode)
        {
            context.Response.StatusCode = statusCode;
            context.Response.Body.Flush();
        }

        /// <summary>
        /// Gets the current entity that we need for serialization and deserialization.
        /// </summary>
        /// <returns></returns>
        private ResourceContext GetCurrentEntity()
        {
            var controllerName = (string)_routeValues["controller"];
            if (controllerName == null)
            {
                return null;
            }
            var resourceType = _controllerResourceMapping.GetAssociatedResource(controllerName);
            var requestResource = _resourceGraph.GetResourceContext(resourceType);
            if (requestResource == null)
            {
                return requestResource;
            }
            if (_routeValues.TryGetValue("relationshipName", out object relationshipName))
            {
                _currentRequest.RequestRelationship = requestResource.Relationships.Single(r => r.PublicRelationshipName == (string)relationshipName);
            }
            return requestResource;
        }
    }
}
