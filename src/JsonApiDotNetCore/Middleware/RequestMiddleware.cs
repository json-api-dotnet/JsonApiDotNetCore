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

        public RequestMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context,
                                 IJsonApiContext jsonApiContext,
                                 IResourceGraph resourceGraph,
                                 IRequestManager requestManager,
                                 IQueryParser queryParser,
                                 IJsonApiOptions options)
        {
            if (IsValid(context))
            {
                // HACK: this currently results in allocation of
                // objects that may or may not be used and even double allocation
                // since the JsonApiContext is using field initializers
                // Need to work on finding a better solution.
                jsonApiContext.BeginOperation();
                ContextEntity contextEntityCurrent = GetCurrentEntity(context.Request.Path, resourceGraph, options);
                requestManager.SetContextEntity(contextEntityCurrent);
                requestManager.BasePath = GetBasePath(context, options, contextEntityCurrent?.EntityName);
                //Handle all querySet
                HandleUriParameters(context, queryParser, requestManager);
                requestManager.IsRelationshipPath = PathIsRelationship(context.Request.Path.Value);
                // BACKWARD COMPATIBILITY for v4  will be removed in v5
                jsonApiContext.RequestManager = requestManager;
                jsonApiContext.PageManager = new PageManager(new LinkBuilder(options, requestManager), options, requestManager);
                await _next(context);
            }
        }
        /// <summary>
        /// Parses the uri, and helps you out
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requestManager"></param>
        protected void HandleUriParameters(HttpContext context, IQueryParser queryParser, IRequestManager requestManager)
        {
            if (context.Request.Query.Count > 0)
            {
                //requestManager.FullQuerySet = context.Request.Query;
                requestManager.QuerySet = queryParser.Parse(context.Request.Query);
                requestManager.IncludedRelationships = requestManager.QuerySet.IncludedRelationships;
            }
        }
        internal static bool PathIsRelationship(string requestPath)
        {
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
        private string GetBasePath(HttpContext context, IJsonApiOptions options, string entityName)
        {
            var r = context.Request;
            if (options.RelativeLinks)
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
        private ContextEntity GetCurrentEntity(PathString path, IResourceGraph resourceGraph, IJsonApiOptions options)
        {
            var pathParsed = path.ToString().Replace($"{options.Namespace}/", "");
            if(pathParsed[0] == '/')
            {
                pathParsed = pathParsed.Substring(1);
            }
            return resourceGraph.GetEntityBasedOnPath(pathParsed);
        }

        private static bool IsValid(HttpContext context)
        {
            return IsValidContentTypeHeader(context) && IsValidAcceptHeader(context);
        }

        private static bool IsValidContentTypeHeader(HttpContext context)
        {
            var contentType = context.Request.ContentType;
            if (contentType != null && ContainsMediaTypeParameters(contentType))
            {
                FlushResponse(context, 415);
                return false;
            }
            return true;
        }

        private static bool IsValidAcceptHeader(HttpContext context)
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

        internal static bool ContainsMediaTypeParameters(string mediaType)
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

        private static void FlushResponse(HttpContext context, int statusCode)
        {
            context.Response.StatusCode = statusCode;
            context.Response.Body.Flush();
        }
    }
}
