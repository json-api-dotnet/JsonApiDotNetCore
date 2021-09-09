using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Diagnostics;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Middleware
{
    /// <summary>
    /// Intercepts HTTP requests to populate injected <see cref="IJsonApiRequest" /> instance for JSON:API requests.
    /// </summary>
    [PublicAPI]
    public sealed class JsonApiMiddleware
    {
        private static readonly MediaTypeHeaderValue MediaType = MediaTypeHeaderValue.Parse(HeaderConstants.MediaType);
        private static readonly MediaTypeHeaderValue AtomicOperationsMediaType = MediaTypeHeaderValue.Parse(HeaderConstants.AtomicOperationsMediaType);

        private readonly RequestDelegate _next;

        public JsonApiMiddleware(RequestDelegate next, IHttpContextAccessor httpContextAccessor)
        {
            _next = next;

            var session = new AspNetCodeTimerSession(httpContextAccessor);
            CodeTimingSessionManager.Capture(session);
        }

        public async Task InvokeAsync(HttpContext httpContext, IControllerResourceMapping controllerResourceMapping, IJsonApiOptions options,
            IJsonApiRequest request, IResourceGraph resourceGraph, ILogger<JsonApiMiddleware> logger)
        {
            ArgumentGuard.NotNull(httpContext, nameof(httpContext));
            ArgumentGuard.NotNull(controllerResourceMapping, nameof(controllerResourceMapping));
            ArgumentGuard.NotNull(options, nameof(options));
            ArgumentGuard.NotNull(request, nameof(request));
            ArgumentGuard.NotNull(resourceGraph, nameof(resourceGraph));
            ArgumentGuard.NotNull(logger, nameof(logger));

            using (CodeTimingSessionManager.Current.Measure("JSON:API middleware"))
            {
                if (!await ValidateIfMatchHeaderAsync(httpContext, options.SerializerSettings))
                {
                    return;
                }

                RouteValueDictionary routeValues = httpContext.GetRouteData().Values;
                ResourceContext primaryResourceContext = TryCreatePrimaryResourceContext(httpContext, controllerResourceMapping, resourceGraph);

                if (primaryResourceContext != null)
                {
                    if (!await ValidateContentTypeHeaderAsync(HeaderConstants.MediaType, httpContext, options.SerializerSettings) ||
                        !await ValidateAcceptHeaderAsync(MediaType, httpContext, options.SerializerSettings))
                    {
                        return;
                    }

                    SetupResourceRequest((JsonApiRequest)request, primaryResourceContext, routeValues, resourceGraph, httpContext.Request);

                    httpContext.RegisterJsonApiRequest();
                }
                else if (IsRouteForOperations(routeValues))
                {
                    if (!await ValidateContentTypeHeaderAsync(HeaderConstants.AtomicOperationsMediaType, httpContext, options.SerializerSettings) ||
                        !await ValidateAcceptHeaderAsync(AtomicOperationsMediaType, httpContext, options.SerializerSettings))
                    {
                        return;
                    }

                    SetupOperationsRequest((JsonApiRequest)request, options, httpContext.Request);

                    httpContext.RegisterJsonApiRequest();
                }

                // Workaround for bug https://github.com/dotnet/aspnetcore/issues/33394
                httpContext.Features.Set<IQueryFeature>(new FixedQueryFeature(httpContext.Features));

                using (CodeTimingSessionManager.Current.Measure("Subsequent middleware"))
                {
                    await _next(httpContext);
                }
            }

            if (CodeTimingSessionManager.IsEnabled)
            {
                string timingResults = CodeTimingSessionManager.Current.GetResults();
                string url = httpContext.Request.GetDisplayUrl();
                logger.LogInformation($"Measurement results for {httpContext.Request.Method} {url}:{Environment.NewLine}{timingResults}");
            }
        }

        private async Task<bool> ValidateIfMatchHeaderAsync(HttpContext httpContext, JsonSerializerSettings serializerSettings)
        {
            if (httpContext.Request.Headers.ContainsKey(HeaderNames.IfMatch))
            {
                await FlushResponseAsync(httpContext.Response, serializerSettings, new ErrorObject(HttpStatusCode.PreconditionFailed)
                {
                    Title = "Detection of mid-air edit collisions using ETags is not supported.",
                    Source = new ErrorSource
                    {
                        Header = "If-Match"
                    }
                });

                return false;
            }

            return true;
        }

        private static ResourceContext TryCreatePrimaryResourceContext(HttpContext httpContext, IControllerResourceMapping controllerResourceMapping,
            IResourceGraph resourceGraph)
        {
            Endpoint endpoint = httpContext.GetEndpoint();
            var controllerActionDescriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();

            if (controllerActionDescriptor != null)
            {
                Type controllerType = controllerActionDescriptor.ControllerTypeInfo;
                Type resourceType = controllerResourceMapping.GetResourceTypeForController(controllerType);

                if (resourceType != null)
                {
                    return resourceGraph.GetResourceContext(resourceType);
                }
            }

            return null;
        }

        private static async Task<bool> ValidateContentTypeHeaderAsync(string allowedContentType, HttpContext httpContext,
            JsonSerializerSettings serializerSettings)
        {
            string contentType = httpContext.Request.ContentType;

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            // Justification: Workaround for https://github.com/dotnet/aspnetcore/issues/32097 (fixed in .NET 6)
            if (contentType != null && contentType != allowedContentType)
            {
                await FlushResponseAsync(httpContext.Response, serializerSettings, new ErrorObject(HttpStatusCode.UnsupportedMediaType)
                {
                    Title = "The specified Content-Type header value is not supported.",
                    Detail = $"Please specify '{allowedContentType}' instead of '{contentType}' for the Content-Type header value.",
                    Source = new ErrorSource
                    {
                        Header = "Content-Type"
                    }
                });

                return false;
            }

            return true;
        }

        private static async Task<bool> ValidateAcceptHeaderAsync(MediaTypeHeaderValue allowedMediaTypeValue, HttpContext httpContext,
            JsonSerializerSettings serializerSettings)
        {
            string[] acceptHeaders = httpContext.Request.Headers.GetCommaSeparatedValues("Accept");

            if (!acceptHeaders.Any())
            {
                return true;
            }

            bool seenCompatibleMediaType = false;

            foreach (string acceptHeader in acceptHeaders)
            {
                if (MediaTypeHeaderValue.TryParse(acceptHeader, out MediaTypeHeaderValue headerValue))
                {
                    headerValue.Quality = null;

                    if (headerValue.MediaType == "*/*" || headerValue.MediaType == "application/*")
                    {
                        seenCompatibleMediaType = true;
                        break;
                    }

                    if (allowedMediaTypeValue.Equals(headerValue))
                    {
                        seenCompatibleMediaType = true;
                        break;
                    }
                }
            }

            if (!seenCompatibleMediaType)
            {
                await FlushResponseAsync(httpContext.Response, serializerSettings, new ErrorObject(HttpStatusCode.NotAcceptable)
                {
                    Title = "The specified Accept header value does not contain any supported media types.",
                    Detail = $"Please include '{allowedMediaTypeValue}' in the Accept header values.",
                    Source = new ErrorSource
                    {
                        Header = "Accept"
                    }
                });

                return false;
            }

            return true;
        }

        private static async Task FlushResponseAsync(HttpResponse httpResponse, JsonSerializerSettings serializerSettings, ErrorObject error)
        {
            httpResponse.ContentType = HeaderConstants.MediaType;
            httpResponse.StatusCode = (int)error.StatusCode;

            var errorDocument = new Document
            {
                Errors = error.AsList()
            };

            var serializer = JsonSerializer.CreateDefault(serializerSettings);
            serializer.ApplyErrorSettings();

            // https://github.com/JamesNK/Newtonsoft.Json/issues/1193
            await using (var stream = new MemoryStream())
            {
                await using (var streamWriter = new StreamWriter(stream, leaveOpen: true))
                {
                    using var jsonWriter = new JsonTextWriter(streamWriter);
                    serializer.Serialize(jsonWriter, errorDocument);
                }

                stream.Seek(0, SeekOrigin.Begin);
                await stream.CopyToAsync(httpResponse.Body);
            }

            await httpResponse.Body.FlushAsync();
        }

        private static void SetupResourceRequest(JsonApiRequest request, ResourceContext primaryResourceContext, RouteValueDictionary routeValues,
            IResourceGraph resourceGraph, HttpRequest httpRequest)
        {
            request.IsReadOnly = httpRequest.Method == HttpMethod.Get.Method || httpRequest.Method == HttpMethod.Head.Method;
            request.PrimaryResource = primaryResourceContext;
            request.PrimaryId = GetPrimaryRequestId(routeValues);

            string relationshipName = GetRelationshipNameForSecondaryRequest(routeValues);

            if (relationshipName != null)
            {
                request.Kind = IsRouteForRelationship(routeValues) ? EndpointKind.Relationship : EndpointKind.Secondary;

                // @formatter:wrap_chained_method_calls chop_always
                // @formatter:keep_existing_linebreaks true

                request.WriteOperation =
                    httpRequest.Method == HttpMethod.Post.Method ? WriteOperationKind.AddToRelationship :
                    httpRequest.Method == HttpMethod.Patch.Method ? WriteOperationKind.SetRelationship :
                    httpRequest.Method == HttpMethod.Delete.Method ? WriteOperationKind.RemoveFromRelationship : null;

                // @formatter:keep_existing_linebreaks restore
                // @formatter:wrap_chained_method_calls restore

                RelationshipAttribute requestRelationship = primaryResourceContext.TryGetRelationshipByPublicName(relationshipName);

                if (requestRelationship != null)
                {
                    request.Relationship = requestRelationship;
                    request.SecondaryResource = resourceGraph.GetResourceContext(requestRelationship.RightType);
                }
            }
            else
            {
                request.Kind = EndpointKind.Primary;

                // @formatter:wrap_chained_method_calls chop_always
                // @formatter:keep_existing_linebreaks true

                request.WriteOperation =
                    httpRequest.Method == HttpMethod.Post.Method ? WriteOperationKind.CreateResource :
                    httpRequest.Method == HttpMethod.Patch.Method ? WriteOperationKind.UpdateResource :
                    httpRequest.Method == HttpMethod.Delete.Method ? WriteOperationKind.DeleteResource : null;

                // @formatter:keep_existing_linebreaks restore
                // @formatter:wrap_chained_method_calls restore
            }

            bool isGetAll = request.PrimaryId == null && request.IsReadOnly;
            request.IsCollection = isGetAll || request.Relationship is HasManyAttribute;
        }

        private static string GetPrimaryRequestId(RouteValueDictionary routeValues)
        {
            return routeValues.TryGetValue("id", out object id) ? (string)id : null;
        }

        private static string GetRelationshipNameForSecondaryRequest(RouteValueDictionary routeValues)
        {
            return routeValues.TryGetValue("relationshipName", out object routeValue) ? (string)routeValue : null;
        }

        private static bool IsRouteForRelationship(RouteValueDictionary routeValues)
        {
            string actionName = (string)routeValues["action"];
            return actionName.EndsWith("Relationship", StringComparison.Ordinal);
        }

        private static bool IsRouteForOperations(RouteValueDictionary routeValues)
        {
            string actionName = (string)routeValues["action"];
            return actionName == "PostOperations";
        }

        private static void SetupOperationsRequest(JsonApiRequest request, IJsonApiOptions options, HttpRequest httpRequest)
        {
            request.IsReadOnly = false;
            request.Kind = EndpointKind.AtomicOperations;
        }
    }
}
