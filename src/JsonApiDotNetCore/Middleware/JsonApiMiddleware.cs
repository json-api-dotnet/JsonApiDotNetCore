using System.Net;
using System.Text.Json;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Diagnostics;
using JsonApiDotNetCore.Errors;
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
    private readonly RequestDelegate? _next;
    private readonly IControllerResourceMapping _controllerResourceMapping;
    private readonly IJsonApiOptions _options;
    private readonly IJsonApiContentNegotiator _contentNegotiator;
    private readonly ILogger<JsonApiMiddleware> _logger;

    public JsonApiMiddleware(RequestDelegate? next, IHttpContextAccessor httpContextAccessor, IControllerResourceMapping controllerResourceMapping,
        IJsonApiOptions options, IJsonApiContentNegotiator contentNegotiator, ILogger<JsonApiMiddleware> logger)
    {
        ArgumentGuard.NotNull(httpContextAccessor);
        ArgumentGuard.NotNull(controllerResourceMapping);
        ArgumentGuard.NotNull(options);
        ArgumentGuard.NotNull(contentNegotiator);
        ArgumentGuard.NotNull(logger);

        _next = next;
        _controllerResourceMapping = controllerResourceMapping;
        _options = options;
        _contentNegotiator = contentNegotiator;
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
            RouteValueDictionary routeValues = httpContext.GetRouteData().Values;
            ResourceType? primaryResourceType = CreatePrimaryResourceType(httpContext, _controllerResourceMapping);

            bool isResourceRequest = primaryResourceType != null;
            bool isOperationsRequest = IsRouteForOperations(routeValues);

            if (isResourceRequest || isOperationsRequest)
            {
                try
                {
                    ValidateIfMatchHeader(httpContext.Request);
                    IReadOnlySet<JsonApiExtension> extensions = _contentNegotiator.Negotiate();

                    if (isResourceRequest)
                    {
                        SetupResourceRequest((JsonApiRequest)request, primaryResourceType!, routeValues, httpContext.Request, extensions);
                    }
                    else
                    {
                        SetupOperationsRequest((JsonApiRequest)request, extensions);
                    }

                    httpContext.RegisterJsonApiRequest();
                }
                catch (JsonApiException exception)
                {
                    await FlushResponseAsync(httpContext.Response, _options.SerializerWriteOptions, exception);
                    return;
                }
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

    private void ValidateIfMatchHeader(HttpRequest httpRequest)
    {
        if (httpRequest.Headers.ContainsKey(HeaderNames.IfMatch))
        {
            throw new JsonApiException(new ErrorObject(HttpStatusCode.PreconditionFailed)
            {
                Title = "Detection of mid-air edit collisions using ETags is not supported.",
                Source = new ErrorSource
                {
                    Header = "If-Match"
                }
            });
        }
    }

    private static ResourceType? CreatePrimaryResourceType(HttpContext httpContext, IControllerResourceMapping controllerResourceMapping)
    {
        Endpoint? endpoint = httpContext.GetEndpoint();
        var controllerActionDescriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();

        return controllerActionDescriptor != null
            ? controllerResourceMapping.GetResourceTypeForController(controllerActionDescriptor.ControllerTypeInfo)
            : null;
    }

    private static void SetupResourceRequest(JsonApiRequest request, ResourceType primaryResourceType, RouteValueDictionary routeValues,
        HttpRequest httpRequest, IReadOnlySet<JsonApiExtension> extensions)
    {
        AssertNoAtomicOperationsExtension(extensions);

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
        request.Extensions = extensions;
    }

    private static void AssertNoAtomicOperationsExtension(IReadOnlySet<JsonApiExtension> extensions)
    {
        if (extensions.Contains(JsonApiExtension.AtomicOperations) || extensions.Contains(JsonApiExtension.RelaxedAtomicOperations))
        {
            throw new InvalidOperationException("Incorrect content negotiation implementation detected: Unexpected atomic:operations extension found.");
        }
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

    internal static bool IsRouteForOperations(RouteValueDictionary routeValues)
    {
        string actionName = (string)routeValues["action"]!;
        return actionName == "PostOperations";
    }

    private static void SetupOperationsRequest(JsonApiRequest request, IReadOnlySet<JsonApiExtension> extensions)
    {
        AssertHasAtomicOperationsExtension(extensions);

        request.IsReadOnly = false;
        request.Kind = EndpointKind.AtomicOperations;
        request.Extensions = extensions;
    }

    private static void AssertHasAtomicOperationsExtension(IReadOnlySet<JsonApiExtension> extensions)
    {
        if (!extensions.Contains(JsonApiExtension.AtomicOperations) && !extensions.Contains(JsonApiExtension.RelaxedAtomicOperations))
        {
            throw new InvalidOperationException("Incorrect content negotiation implementation detected: Missing atomic:operations extension.");
        }
    }

    private static async Task FlushResponseAsync(HttpResponse httpResponse, JsonSerializerOptions serializerOptions, JsonApiException exception)
    {
        httpResponse.ContentType = JsonApiMediaType.Default.ToString();
        httpResponse.StatusCode = (int)ErrorObject.GetResponseStatusCode(exception.Errors);

        var errorDocument = new Document
        {
            Errors = exception.Errors.ToList()
        };

        await JsonSerializer.SerializeAsync(httpResponse.Body, errorDocument, serializerOptions);
        await httpResponse.Body.FlushAsync();
    }

    [LoggerMessage(Level = LogLevel.Information, SkipEnabledCheck = true,
        Message = "Measurement results for {RequestMethod} {RequestUrl}:{LineBreak}{TimingResults}")]
    private partial void LogMeasurement(string requestMethod, string requestUrl, string lineBreak, string timingResults);
}
