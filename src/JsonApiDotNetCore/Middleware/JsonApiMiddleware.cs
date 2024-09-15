using System.Net;
using System.Text.Json;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Diagnostics;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace JsonApiDotNetCore.Middleware;

/// <summary>
/// Intercepts HTTP requests to populate injected <see cref="IJsonApiRequest" /> instance for JSON:API requests.
/// </summary>
[PublicAPI]
public sealed partial class JsonApiMiddleware
{
    private static readonly string[] NonOperationsContentTypes = [HeaderConstants.MediaType];
    private static readonly MediaTypeHeaderValue[] NonOperationsMediaTypes = [MediaTypeHeaderValue.Parse(HeaderConstants.MediaType)];

    private static readonly string[] OperationsContentTypes =
    [
        HeaderConstants.AtomicOperationsMediaType,
        HeaderConstants.RelaxedAtomicOperationsMediaType
    ];

    private static readonly MediaTypeHeaderValue[] OperationsMediaTypes =
    [
        MediaTypeHeaderValue.Parse(HeaderConstants.AtomicOperationsMediaType),
        MediaTypeHeaderValue.Parse(HeaderConstants.RelaxedAtomicOperationsMediaType)
    ];

    private readonly RequestDelegate? _next;
    private readonly IControllerResourceMapping _controllerResourceMapping;
    private readonly IJsonApiOptions _options;
    private readonly ILogger<JsonApiMiddleware> _logger;

    public JsonApiMiddleware(RequestDelegate? next, IHttpContextAccessor httpContextAccessor, IControllerResourceMapping controllerResourceMapping,
        IJsonApiOptions options, ILogger<JsonApiMiddleware> logger)
    {
        ArgumentGuard.NotNull(httpContextAccessor);
        ArgumentGuard.NotNull(controllerResourceMapping);
        ArgumentGuard.NotNull(options);
        ArgumentGuard.NotNull(logger);

        _next = next;
        _controllerResourceMapping = controllerResourceMapping;
        _options = options;
        _logger = logger;

#pragma warning disable CA2000 // Dispose objects before losing scope
        var session = new AspNetCodeTimerSession(httpContextAccessor);
#pragma warning restore CA2000 // Dispose objects before losing scope
        CodeTimingSessionManager.Capture(session);
    }

    public async Task InvokeAsync(HttpContext httpContext, IJsonApiRequest request)
    {
        ArgumentGuard.NotNull(httpContext);
        ArgumentGuard.NotNull(request);

        using (CodeTimingSessionManager.Current.Measure("JSON:API middleware"))
        {
            if (!await ValidateIfMatchHeaderAsync(httpContext, _options.SerializerWriteOptions))
            {
                return;
            }

            RouteValueDictionary routeValues = httpContext.GetRouteData().Values;
            ResourceType? primaryResourceType = CreatePrimaryResourceType(httpContext, _controllerResourceMapping);

            if (primaryResourceType != null)
            {
                if (!await ValidateContentTypeHeaderAsync(NonOperationsContentTypes, httpContext, _options.SerializerWriteOptions) ||
                    !await ValidateAcceptHeaderAsync(NonOperationsMediaTypes, httpContext, _options.SerializerWriteOptions))
                {
                    return;
                }

                SetupResourceRequest((JsonApiRequest)request, primaryResourceType, routeValues, httpContext.Request);

                httpContext.RegisterJsonApiRequest();
            }
            else if (IsRouteForOperations(routeValues))
            {
                if (!await ValidateContentTypeHeaderAsync(OperationsContentTypes, httpContext, _options.SerializerWriteOptions) ||
                    !await ValidateAcceptHeaderAsync(OperationsMediaTypes, httpContext, _options.SerializerWriteOptions))
                {
                    return;
                }

                SetupOperationsRequest((JsonApiRequest)request);

                httpContext.RegisterJsonApiRequest();
            }

            if (_next != null)
            {
                using (CodeTimingSessionManager.Current.Measure("Subsequent middleware"))
                {
                    await _next(httpContext);
                }
            }
        }

        if (CodeTimingSessionManager.IsEnabled && _logger.IsEnabled(LogLevel.Information))
        {
            string timingResults = CodeTimingSessionManager.Current.GetResults();
            string requestMethod = httpContext.Request.Method.Replace(Environment.NewLine, "");
            string requestUrl = httpContext.Request.GetEncodedUrl();
            LogMeasurement(requestMethod, requestUrl, Environment.NewLine, timingResults);
        }
    }

    private async Task<bool> ValidateIfMatchHeaderAsync(HttpContext httpContext, JsonSerializerOptions serializerOptions)
    {
        if (httpContext.Request.Headers.ContainsKey(HeaderNames.IfMatch))
        {
            await FlushResponseAsync(httpContext.Response, serializerOptions, new ErrorObject(HttpStatusCode.PreconditionFailed)
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

    private static ResourceType? CreatePrimaryResourceType(HttpContext httpContext, IControllerResourceMapping controllerResourceMapping)
    {
        Endpoint? endpoint = httpContext.GetEndpoint();
        var controllerActionDescriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();

        return controllerActionDescriptor != null
            ? controllerResourceMapping.GetResourceTypeForController(controllerActionDescriptor.ControllerTypeInfo)
            : null;
    }

    private static async Task<bool> ValidateContentTypeHeaderAsync(string[] allowedContentTypes, HttpContext httpContext,
        JsonSerializerOptions serializerOptions)
    {
        string? contentType = httpContext.Request.ContentType;

        if (contentType != null && !allowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
        {
            string allowedValues = string.Join(" or ", allowedContentTypes.Select(value => $"'{value}'"));

            await FlushResponseAsync(httpContext.Response, serializerOptions, new ErrorObject(HttpStatusCode.UnsupportedMediaType)
            {
                Title = "The specified Content-Type header value is not supported.",
                Detail = $"Please specify {allowedValues} instead of '{contentType}' for the Content-Type header value.",
                Source = new ErrorSource
                {
                    Header = "Content-Type"
                }
            });

            return false;
        }

        return true;
    }

    private static async Task<bool> ValidateAcceptHeaderAsync(MediaTypeHeaderValue[] allowedMediaTypes, HttpContext httpContext,
        JsonSerializerOptions serializerOptions)
    {
        string[] acceptHeaders = httpContext.Request.Headers.GetCommaSeparatedValues("Accept");

        if (acceptHeaders.Length == 0)
        {
            return true;
        }

        bool seenCompatibleMediaType = false;

        foreach (string acceptHeader in acceptHeaders)
        {
            if (MediaTypeHeaderValue.TryParse(acceptHeader, out MediaTypeHeaderValue? headerValue))
            {
                if (headerValue.MediaType == "*/*" || headerValue.MediaType == "application/*")
                {
                    seenCompatibleMediaType = true;
                    break;
                }

                headerValue.Quality = null;

                if (allowedMediaTypes.Contains(headerValue))
                {
                    seenCompatibleMediaType = true;
                    break;
                }
            }
        }

        if (!seenCompatibleMediaType)
        {
            string allowedValues = string.Join(" or ", allowedMediaTypes.Select(value => $"'{value}'"));

            await FlushResponseAsync(httpContext.Response, serializerOptions, new ErrorObject(HttpStatusCode.NotAcceptable)
            {
                Title = "The specified Accept header value does not contain any supported media types.",
                Detail = $"Please include {allowedValues} in the Accept header values.",
                Source = new ErrorSource
                {
                    Header = "Accept"
                }
            });

            return false;
        }

        return true;
    }

    private static async Task FlushResponseAsync(HttpResponse httpResponse, JsonSerializerOptions serializerOptions, ErrorObject error)
    {
        httpResponse.ContentType = HeaderConstants.MediaType;
        httpResponse.StatusCode = (int)error.StatusCode;

        var errorDocument = new Document
        {
            Errors = [error]
        };

        await JsonSerializer.SerializeAsync(httpResponse.Body, errorDocument, serializerOptions);
        await httpResponse.Body.FlushAsync();
    }

    private static void SetupResourceRequest(JsonApiRequest request, ResourceType primaryResourceType, RouteValueDictionary routeValues,
        HttpRequest httpRequest)
    {
        request.IsReadOnly = httpRequest.Method == HttpMethod.Get.Method || httpRequest.Method == HttpMethod.Head.Method;
        request.PrimaryResourceType = primaryResourceType;
        request.PrimaryId = GetPrimaryRequestId(routeValues);

        string? relationshipName = GetRelationshipNameForSecondaryRequest(routeValues);

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

            RelationshipAttribute? requestRelationship = primaryResourceType.FindRelationshipByPublicName(relationshipName);

            if (requestRelationship != null)
            {
                request.Relationship = requestRelationship;
                request.SecondaryResourceType = requestRelationship.RightType;
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

    private static string? GetPrimaryRequestId(RouteValueDictionary routeValues)
    {
        return routeValues.TryGetValue("id", out object? id) ? (string?)id : null;
    }

    private static string? GetRelationshipNameForSecondaryRequest(RouteValueDictionary routeValues)
    {
        return routeValues.TryGetValue("relationshipName", out object? routeValue) ? (string?)routeValue : null;
    }

    private static bool IsRouteForRelationship(RouteValueDictionary routeValues)
    {
        string actionName = (string)routeValues["action"]!;
        return actionName.EndsWith("Relationship", StringComparison.Ordinal);
    }

    private static bool IsRouteForOperations(RouteValueDictionary routeValues)
    {
        string actionName = (string)routeValues["action"]!;
        return actionName == "PostOperations";
    }

    private static void SetupOperationsRequest(JsonApiRequest request)
    {
        request.IsReadOnly = false;
        request.Kind = EndpointKind.AtomicOperations;
    }

    [LoggerMessage(Level = LogLevel.Information, SkipEnabledCheck = true,
        Message = "Measurement results for {RequestMethod} {RequestUrl}:{LineBreak}{TimingResults}")]
    private partial void LogMeasurement(string requestMethod, string requestUrl, string lineBreak, string timingResults);
}
