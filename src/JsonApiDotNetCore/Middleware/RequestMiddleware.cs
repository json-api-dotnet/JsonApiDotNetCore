using System;
using System.Linq;
using System.Threading.Tasks;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.Middleware
{
    /// <summary>
    /// Can be overwritten to help you out during testing
    /// 
    /// This sets all necessary paaramters relating to the HttpContext for JADNC
    /// </summary>
    public class RequestMiddleware
    {
        private readonly RequestDelegate _next;
        private IResourceGraph _resourceGraph;
        private HttpContext _httpContext;
        private IJsonApiOptions _options;
        private IJsonApiContext _jsonApiContext;
        private IRequestManager _requestManager;
        private IQueryParser _queryParser;

        public RequestMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext,
                                 IJsonApiContext jsonApiContext,
                                 IResourceGraph resourceGraph,
                                 IRequestManager requestManager,
                                 IQueryParser queryParser,
                                 IJsonApiOptions options)
        {
            _httpContext = httpContext;
            _jsonApiContext = jsonApiContext;
            _resourceGraph = resourceGraph;
            _requestManager = requestManager;
            _queryParser = queryParser;
            _options = options;

            if (IsValid())
            {

                // HACK: this currently results in allocation of
                // objects that may or may not be used and even double allocation
                // since the JsonApiContext is using field initializers
                // Need to work on finding a better solution.
                jsonApiContext.BeginOperation();
                ContextEntity contextEntityCurrent = GetCurrentEntity();
                // the contextEntity is null eg when we're using a non-JsonApiDotNetCore route. 
                if (contextEntityCurrent != null)
                {
                    requestManager.SetContextEntity(contextEntityCurrent);
                    // TODO: this does not need to be reset every request: we shouldn't need to rely on an external request to figure out the basepath of current application
                    requestManager.BasePath = GetBasePath(contextEntityCurrent.EntityName);
                    //Handle all querySet
                    HandleUriParameters();
                    requestManager.IsRelationshipPath = PathIsRelationship();
                    // BACKWARD COMPATIBILITY for v4  will be removed in v5
                    jsonApiContext.RequestManager = requestManager;
                    jsonApiContext.PageManager = new PageManager(new LinkBuilder(options, requestManager), options, requestManager);
                }

                await _next(httpContext);
            }
        }
        /// <summary>
        /// Parses the uri, and helps you out
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requestManager"></param>
        protected void HandleUriParameters()
        {
            if (_httpContext.Request.Query.Count > 0)
            {
                //requestManager.FullQuerySet = context.Request.Query;
                _requestManager.QuerySet = _queryParser.Parse(_httpContext.Request.Query);
                _requestManager.IncludedRelationships = _requestManager.QuerySet.IncludedRelationships;
            }
        }

        protected bool PathIsRelationship()
        {
            string requestPath = _httpContext.Request.Path.Value;
            // while(!Debugger.IsAttached) { Thread.Sleep(1000); }
            const string relationships = "relationships";
            const char pathSegmentDelimiter = '/';

            var span = requestPath.AsSpan();

            // we need to iterate over the string, from the end,
            // checking whether or not the 2nd to last path segment
            // is "relationships"
            // -2 is chosen in case the path ends with '/'
            for (var i = requestPath.Length - 2; i >= 0; i--)
            {
                // if there are not enough characters left in the path to 
                // contain "relationships"
                if (i < relationships.Length)
                    return false;

                // we have found the first instance of '/'
                if (span[i] == pathSegmentDelimiter)
                {
                    // in the case of a "relationships" route, the next
                    // path segment will be "relationships"
                    return (
                        span.Slice(i - relationships.Length, relationships.Length)
                            .SequenceEqual(relationships.AsSpan())
                    );
                }
            }

            return false;
        }
        private string GetBasePath(string entityName)
        {
            var r = _httpContext.Request;
            if (_options.RelativeLinks)
            {
                return GetNamespaceFromPath(r.Path, entityName);
            }
            else
            {
                return $"{r.Scheme}://{r.Host}{GetNamespaceFromPath(r.Path, entityName)}";
            }
        }
        internal static string GetNamespaceFromPath(string path, string entityName)
        {
            var entityNameSpan = entityName.AsSpan();
            var pathSpan = path.AsSpan();
            const char delimiter = '/';
            for (var i = 0; i < pathSpan.Length; i++)
            {
                if (pathSpan[i].Equals(delimiter))
                {
                    var nextPosition = i + 1;
                    if (pathSpan.Length > i + entityNameSpan.Length)
                    {
                        var possiblePathSegment = pathSpan.Slice(nextPosition, entityNameSpan.Length);
                        if (entityNameSpan.SequenceEqual(possiblePathSegment))
                        {
                            // check to see if it's the last position in the string
                            //   or if the next character is a /
                            var lastCharacterPosition = nextPosition + entityNameSpan.Length;

                            if (lastCharacterPosition == pathSpan.Length || pathSpan.Length >= lastCharacterPosition + 2 && pathSpan[lastCharacterPosition].Equals(delimiter))
                            {
                                return pathSpan.Slice(0, i).ToString();
                            }
                        }
                    }
                }
            }

            return string.Empty;
        }
        /// <summary>
        /// Gets the current entity that we need for serialization and deserialization.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="resourceGraph"></param>
        /// <returns></returns>
        private ContextEntity GetCurrentEntity()
        {
            var controllerName = (string)_httpContext.GetRouteData().Values["controller"];
            return _resourceGraph.GetEntityFromControllerName(controllerName);
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
                    continue;

                FlushResponse(context, 406);
                return false;
            }
            return true;
        }

        internal bool ContainsMediaTypeParameters(string mediaType)
        {
            var incomingMediaTypeSpan = mediaType.AsSpan();

            // if the content type is not application/vnd.api+json then continue on
            if (incomingMediaTypeSpan.Length < Constants.ContentType.Length)
                return false;

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
    }
}
