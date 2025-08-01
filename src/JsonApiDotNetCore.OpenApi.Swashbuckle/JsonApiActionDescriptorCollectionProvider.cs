using System.Net;
using System.Reflection;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata.ActionMethods;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata.Documents;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Documents;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

/// <summary>
/// Adds OpenAPI metadata (consumes, produces) to <see cref="ActionDescriptor" />s and performs endpoint expansion for secondary and relationship
/// endpoints. For example: <code><![CDATA[
/// /article/{id}/{relationshipName} -> /article/{id}/author, /article/{id}/revisions, etc.
/// ]]></code>
/// </summary>
internal sealed partial class JsonApiActionDescriptorCollectionProvider : IActionDescriptorCollectionProvider
{
    private const int FilterScope = 10;
    private static readonly Type ErrorDocumentType = typeof(ErrorResponseDocument);

    private readonly IActionDescriptorCollectionProvider _defaultProvider;
    private readonly IControllerResourceMapping _controllerResourceMapping;
    private readonly JsonApiEndpointMetadataProvider _jsonApiEndpointMetadataProvider;
    private readonly IJsonApiOptions _options;
    private readonly ILogger<JsonApiActionDescriptorCollectionProvider> _logger;

    public ActionDescriptorCollection ActionDescriptors => GetActionDescriptors();

    public JsonApiActionDescriptorCollectionProvider(IActionDescriptorCollectionProvider defaultProvider, IControllerResourceMapping controllerResourceMapping,
        JsonApiEndpointMetadataProvider jsonApiEndpointMetadataProvider, IJsonApiOptions options, ILogger<JsonApiActionDescriptorCollectionProvider> logger)
    {
        ArgumentNullException.ThrowIfNull(defaultProvider);
        ArgumentNullException.ThrowIfNull(controllerResourceMapping);
        ArgumentNullException.ThrowIfNull(jsonApiEndpointMetadataProvider);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _defaultProvider = defaultProvider;
        _controllerResourceMapping = controllerResourceMapping;
        _jsonApiEndpointMetadataProvider = jsonApiEndpointMetadataProvider;
        _options = options;
        _logger = logger;
    }

    private ActionDescriptorCollection GetActionDescriptors()
    {
        List<ActionDescriptor> descriptors = [];

        foreach (ActionDescriptor descriptor in _defaultProvider.ActionDescriptors.Items)
        {
            if (!descriptor.EndpointMetadata.OfType<IHttpMethodMetadata>().SelectMany(metadata => metadata.HttpMethods).Any())
            {
                // Technically incorrect: when no verbs, the endpoint is exposed on all verbs. But Swashbuckle hides it anyway.
                continue;
            }

            var actionMethod = OpenApiActionMethod.Create(descriptor);

            if (actionMethod is CustomJsonApiActionMethod)
            {
                // A non-standard action method in a JSON:API controller. Not yet implemented, so skip to prevent downstream crashes.
                string httpMethods = string.Join(", ", descriptor.EndpointMetadata.OfType<IHttpMethodMetadata>().SelectMany(metadata => metadata.HttpMethods));
                LogSuppressedActionMethod(httpMethods, descriptor.DisplayName);

                continue;
            }

            if (actionMethod is BuiltinJsonApiActionMethod builtinActionMethod)
            {
                if (!IsVisibleEndpoint(descriptor))
                {
                    continue;
                }

                ResourceType? resourceType = _controllerResourceMapping.GetResourceTypeForController(builtinActionMethod.ControllerType);

                if (builtinActionMethod is JsonApiActionMethod jsonApiActionMethod)
                {
                    ConsistencyGuard.ThrowIf(resourceType == null);

                    if (ShouldSuppressEndpoint(jsonApiActionMethod.Endpoint, resourceType))
                    {
                        continue;
                    }
                }

                ActionDescriptor[] replacementDescriptors = SetEndpointMetadata(descriptor, builtinActionMethod, resourceType);
                descriptors.AddRange(replacementDescriptors);

                continue;
            }

            descriptors.Add(descriptor);
        }

        int descriptorVersion = _defaultProvider.ActionDescriptors.Version;
        return new ActionDescriptorCollection(descriptors.AsReadOnly(), descriptorVersion);
    }

    internal static bool IsVisibleEndpoint(ActionDescriptor descriptor)
    {
        // Only if in a convention ApiExplorer.IsVisible was set to false, the ApiDescriptionActionData will not be present.
        return descriptor is ControllerActionDescriptor controllerDescriptor && controllerDescriptor.Properties.ContainsKey(typeof(ApiDescriptionActionData));
    }

    private static bool ShouldSuppressEndpoint(JsonApiEndpoints endpoint, ResourceType resourceType)
    {
        if (!IsEndpointAvailable(endpoint, resourceType))
        {
            return true;
        }

        if (IsSecondaryOrRelationshipEndpoint(endpoint))
        {
            if (resourceType.Relationships.Count == 0)
            {
                return true;
            }

            if (endpoint is JsonApiEndpoints.DeleteRelationship or JsonApiEndpoints.PostRelationship)
            {
                return !resourceType.Relationships.OfType<HasManyAttribute>().Any();
            }
        }

        return false;
    }

    private static bool IsEndpointAvailable(JsonApiEndpoints endpoint, ResourceType resourceType)
    {
        JsonApiEndpoints availableEndpoints = GetGeneratedControllerEndpoints(resourceType);

        if (availableEndpoints == JsonApiEndpoints.None)
        {
            // Auto-generated controllers are disabled, so we can't know what to hide.
            // It is assumed that a handwritten JSON:API controller only provides action methods for what it supports.
            // To accomplish that, derive from BaseJsonApiController instead of JsonApiController.
            return true;
        }

        // For an overridden JSON:API action method in a partial class to show up, it's flag must be turned on in [Resource].
        // Otherwise, it is considered to be an action method that throws because the endpoint is unavailable.
        return IncludesEndpoint(endpoint, availableEndpoints);
    }

    private static JsonApiEndpoints GetGeneratedControllerEndpoints(ResourceType resourceType)
    {
        var resourceAttribute = resourceType.ClrType.GetCustomAttribute<ResourceAttribute>();
        return resourceAttribute?.GenerateControllerEndpoints ?? JsonApiEndpoints.None;
    }

    private static bool IncludesEndpoint(JsonApiEndpoints endpoint, JsonApiEndpoints availableEndpoints)
    {
        bool? isIncluded = null;

        if (endpoint == JsonApiEndpoints.GetCollection)
        {
            isIncluded = availableEndpoints.HasFlag(JsonApiEndpoints.GetCollection);
        }
        else if (endpoint == JsonApiEndpoints.GetSingle)
        {
            isIncluded = availableEndpoints.HasFlag(JsonApiEndpoints.GetSingle);
        }
        else if (endpoint == JsonApiEndpoints.GetSecondary)
        {
            isIncluded = availableEndpoints.HasFlag(JsonApiEndpoints.GetSecondary);
        }
        else if (endpoint == JsonApiEndpoints.GetRelationship)
        {
            isIncluded = availableEndpoints.HasFlag(JsonApiEndpoints.GetRelationship);
        }
        else if (endpoint == JsonApiEndpoints.Post)
        {
            isIncluded = availableEndpoints.HasFlag(JsonApiEndpoints.Post);
        }
        else if (endpoint == JsonApiEndpoints.PostRelationship)
        {
            isIncluded = availableEndpoints.HasFlag(JsonApiEndpoints.PostRelationship);
        }
        else if (endpoint == JsonApiEndpoints.Patch)
        {
            isIncluded = availableEndpoints.HasFlag(JsonApiEndpoints.Patch);
        }
        else if (endpoint == JsonApiEndpoints.PatchRelationship)
        {
            isIncluded = availableEndpoints.HasFlag(JsonApiEndpoints.PatchRelationship);
        }
        else if (endpoint == JsonApiEndpoints.Delete)
        {
            isIncluded = availableEndpoints.HasFlag(JsonApiEndpoints.Delete);
        }
        else if (endpoint == JsonApiEndpoints.DeleteRelationship)
        {
            isIncluded = availableEndpoints.HasFlag(JsonApiEndpoints.DeleteRelationship);
        }

        ConsistencyGuard.ThrowIf(isIncluded == null);
        return isIncluded.Value;
    }

    private static bool IsSecondaryOrRelationshipEndpoint(JsonApiEndpoints endpoint)
    {
        return endpoint is JsonApiEndpoints.GetSecondary or JsonApiEndpoints.GetRelationship or JsonApiEndpoints.PostRelationship or
            JsonApiEndpoints.PatchRelationship or JsonApiEndpoints.DeleteRelationship;
    }

    private ActionDescriptor[] SetEndpointMetadata(ActionDescriptor descriptor, BuiltinJsonApiActionMethod actionMethod, ResourceType? resourceType)
    {
        Dictionary<RelationshipAttribute, ActionDescriptor> descriptorsByRelationship = [];

        JsonApiEndpointMetadata endpointMetadata = _jsonApiEndpointMetadataProvider.Get(descriptor);

        switch (endpointMetadata.RequestMetadata)
        {
            case AtomicOperationsRequestMetadata atomicOperationsRequestMetadata:
            {
                SetConsumes(descriptor, atomicOperationsRequestMetadata.DocumentType, JsonApiMediaType.AtomicOperations);
                UpdateRequestBodyParameterDescriptor(descriptor, atomicOperationsRequestMetadata.DocumentType, null);

                break;
            }
            case PrimaryRequestMetadata primaryRequestMetadata:
            {
                SetConsumes(descriptor, primaryRequestMetadata.DocumentType, JsonApiMediaType.Default);
                UpdateRequestBodyParameterDescriptor(descriptor, primaryRequestMetadata.DocumentType, null);

                break;
            }
            case RelationshipRequestMetadata relationshipRequestMetadata:
            {
                ConsistencyGuard.ThrowIf(descriptor.AttributeRouteInfo == null);

                foreach ((RelationshipAttribute relationship, Type documentType) in relationshipRequestMetadata.DocumentTypesByRelationship)
                {
                    ActionDescriptor relationshipDescriptor = Clone(descriptor);

                    RemovePathParameter(relationshipDescriptor.Parameters, "relationshipName");
                    ExpandTemplate(relationshipDescriptor.AttributeRouteInfo!, relationship.PublicName);
                    SetConsumes(descriptor, documentType, JsonApiMediaType.Default);
                    UpdateRequestBodyParameterDescriptor(relationshipDescriptor, documentType, relationship.PublicName);

                    descriptorsByRelationship[relationship] = relationshipDescriptor;
                }

                break;
            }
        }

        switch (endpointMetadata.ResponseMetadata)
        {
            case AtomicOperationsResponseMetadata atomicOperationsResponseMetadata:
            {
                SetProduces(descriptor, atomicOperationsResponseMetadata.DocumentType);
                SetProducesResponseTypes(descriptor, actionMethod, resourceType, atomicOperationsResponseMetadata.DocumentType);

                break;
            }
            case PrimaryResponseMetadata primaryResponseMetadata:
            {
                SetProduces(descriptor, primaryResponseMetadata.DocumentType);
                SetProducesResponseTypes(descriptor, actionMethod, resourceType, primaryResponseMetadata.DocumentType);
                break;
            }
            case NonPrimaryResponseMetadata nonPrimaryResponseMetadata:
            {
                foreach ((RelationshipAttribute relationship, Type documentType) in nonPrimaryResponseMetadata.DocumentTypesByRelationship)
                {
                    SetNonPrimaryResponseMetadata(descriptor, actionMethod, resourceType, descriptorsByRelationship, relationship, documentType);
                }

                break;
            }
            case EmptyRelationshipResponseMetadata emptyRelationshipResponseMetadata:
            {
                foreach (RelationshipAttribute relationship in emptyRelationshipResponseMetadata.Relationships)
                {
                    SetNonPrimaryResponseMetadata(descriptor, actionMethod, resourceType, descriptorsByRelationship, relationship, null);
                }

                break;
            }
        }

        return descriptorsByRelationship.Count == 0 ? [descriptor] : descriptorsByRelationship.Values.ToArray();
    }

    private static void SetConsumes(ActionDescriptor descriptor, Type requestType, JsonApiMediaType mediaType)
    {
        // This value doesn't actually appear in the OpenAPI document, but is only used to invoke
        // JsonApiRequestFormatMetadataProvider.GetSupportedContentTypes(), which determines the actual request content type.
        string contentType = mediaType.ToString();

        descriptor.FilterDescriptors.Add(new FilterDescriptor(new ConsumesAttribute(requestType, contentType), FilterScope));
    }

    private static void UpdateRequestBodyParameterDescriptor(ActionDescriptor descriptor, Type documentType, string? parameterName)
    {
        ControllerParameterDescriptor? requestBodyDescriptor = descriptor.GetBodyParameterDescriptor();

        if (requestBodyDescriptor == null)
        {
            MethodInfo actionMethod = descriptor.GetActionMethod();

            throw new InvalidConfigurationException(
                $"The action method '{actionMethod}' on type '{actionMethod.ReflectedType?.FullName}' contains no parameter with a [FromBody] attribute.");
        }

        descriptor.EndpointMetadata.Add(new ConsumesAttribute(JsonApiMediaType.Default.ToString()));

        requestBodyDescriptor.ParameterType = documentType;
        requestBodyDescriptor.ParameterInfo = new ParameterInfoWrapper(requestBodyDescriptor.ParameterInfo, documentType, parameterName);
    }

    private static ActionDescriptor Clone(ActionDescriptor descriptor)
    {
        ActionDescriptor clone = descriptor.MemberwiseClone();
        clone.AttributeRouteInfo = descriptor.AttributeRouteInfo!.MemberwiseClone();
        clone.FilterDescriptors = descriptor.FilterDescriptors.Select(Clone).ToList();
        clone.Parameters = descriptor.Parameters.Select(parameter => parameter.MemberwiseClone()).ToList();
        return clone;
    }

    private static FilterDescriptor Clone(FilterDescriptor descriptor)
    {
        IFilterMetadata clone = descriptor.Filter.MemberwiseClone();

        return new FilterDescriptor(clone, descriptor.Scope)
        {
            Order = descriptor.Order
        };
    }

    private static void RemovePathParameter(ICollection<ParameterDescriptor> parameters, string parameterName)
    {
        ParameterDescriptor descriptor = parameters.Single(parameterDescriptor => parameterDescriptor.Name == parameterName);
        parameters.Remove(descriptor);
    }

    private static void ExpandTemplate(AttributeRouteInfo route, string parameterName)
    {
        route.Template = route.Template!.Replace("{relationshipName}", parameterName);
    }

    private void SetProduces(ActionDescriptor descriptor, Type? documentType)
    {
        IReadOnlyList<string> contentTypes = OpenApiContentTypeProvider.Instance.GetResponseContentTypes(documentType);

        if (contentTypes.Count > 0)
        {
            descriptor.FilterDescriptors.Add(new FilterDescriptor(new ProducesAttribute(contentTypes[0]), FilterScope));
        }
    }

    private void SetProducesResponseTypes(ActionDescriptor descriptor, BuiltinJsonApiActionMethod actionMethod, ResourceType? resourceType, Type? documentType)
    {
        foreach (HttpStatusCode statusCode in GetSuccessStatusCodesForActionMethod(actionMethod))
        {
            descriptor.FilterDescriptors.Add(documentType == null || StatusCodeHasNoResponseBody(statusCode)
                ? new FilterDescriptor(new ProducesResponseTypeAttribute(typeof(void), (int)statusCode), FilterScope)
                : new FilterDescriptor(new ProducesResponseTypeAttribute(documentType, (int)statusCode), FilterScope));
        }

        string? errorContentType = null;

        if (documentType == null)
        {
            IReadOnlyList<string> errorContentTypes = OpenApiContentTypeProvider.Instance.GetResponseContentTypes(ErrorDocumentType);
            ConsistencyGuard.ThrowIf(errorContentTypes.Count == 0);
            errorContentType = errorContentTypes[0];
        }

        foreach (HttpStatusCode statusCode in GetErrorStatusCodesForActionMethod(actionMethod, resourceType))
        {
            descriptor.FilterDescriptors.Add(errorContentType != null
                ? new FilterDescriptor(new ProducesResponseTypeAttribute(ErrorDocumentType, (int)statusCode, errorContentType), FilterScope)
                : new FilterDescriptor(new ProducesResponseTypeAttribute(ErrorDocumentType, (int)statusCode), FilterScope));
        }
    }

    private static HttpStatusCode[] GetSuccessStatusCodesForActionMethod(BuiltinJsonApiActionMethod actionMethod)
    {
        HttpStatusCode[]? statusCodes = null;

        if (actionMethod is AtomicOperationsActionMethod)
        {
            statusCodes =
            [
                HttpStatusCode.OK,
                HttpStatusCode.NoContent
            ];
        }
        else if (actionMethod is JsonApiActionMethod jsonApiActionMethod)
        {
            statusCodes = jsonApiActionMethod.Endpoint switch
            {
                JsonApiEndpoints.GetCollection or JsonApiEndpoints.GetSingle or JsonApiEndpoints.GetSecondary or JsonApiEndpoints.GetRelationship =>
                [
                    HttpStatusCode.OK,
                    HttpStatusCode.NotModified
                ],
                JsonApiEndpoints.Post =>
                [
                    HttpStatusCode.Created,
                    HttpStatusCode.NoContent
                ],
                JsonApiEndpoints.Patch =>
                [
                    HttpStatusCode.OK,
                    HttpStatusCode.NoContent
                ],
                JsonApiEndpoints.Delete or JsonApiEndpoints.PostRelationship or JsonApiEndpoints.PatchRelationship or JsonApiEndpoints.DeleteRelationship =>
                [
                    HttpStatusCode.NoContent
                ],
                _ => null
            };
        }

        ConsistencyGuard.ThrowIf(statusCodes == null);
        return statusCodes;
    }

    private static bool StatusCodeHasNoResponseBody(HttpStatusCode statusCode)
    {
        return statusCode is HttpStatusCode.NoContent or HttpStatusCode.NotModified;
    }

    private HttpStatusCode[] GetErrorStatusCodesForActionMethod(BuiltinJsonApiActionMethod actionMethod, ResourceType? resourceType)
    {
        HttpStatusCode[]? statusCodes = null;

        if (actionMethod is AtomicOperationsActionMethod)
        {
            statusCodes =
            [
                HttpStatusCode.BadRequest,
                HttpStatusCode.Forbidden,
                HttpStatusCode.NotFound,
                HttpStatusCode.Conflict,
                HttpStatusCode.UnprocessableEntity
            ];
        }
        else if (actionMethod is JsonApiActionMethod jsonApiActionMethod)
        {
            // Condition doesn't apply to atomic operations, because Forbidden is also used when an operation is not accessible.
            ClientIdGenerationMode clientIdGeneration = resourceType?.ClientIdGeneration ?? _options.ClientIdGeneration;

            statusCodes = jsonApiActionMethod.Endpoint switch
            {
                JsonApiEndpoints.GetCollection => [HttpStatusCode.BadRequest],
                JsonApiEndpoints.GetSingle or JsonApiEndpoints.GetSecondary or JsonApiEndpoints.GetRelationship =>
                [
                    HttpStatusCode.BadRequest,
                    HttpStatusCode.NotFound
                ],
                JsonApiEndpoints.Post when clientIdGeneration == ClientIdGenerationMode.Forbidden =>
                [
                    HttpStatusCode.BadRequest,
                    HttpStatusCode.Forbidden,
                    HttpStatusCode.NotFound,
                    HttpStatusCode.Conflict,
                    HttpStatusCode.UnprocessableEntity
                ],
                JsonApiEndpoints.Post or JsonApiEndpoints.Patch =>
                [
                    HttpStatusCode.BadRequest,
                    HttpStatusCode.NotFound,
                    HttpStatusCode.Conflict,
                    HttpStatusCode.UnprocessableEntity
                ],
                JsonApiEndpoints.Delete => [HttpStatusCode.NotFound],
                JsonApiEndpoints.PostRelationship or JsonApiEndpoints.PatchRelationship or JsonApiEndpoints.DeleteRelationship =>
                [
                    HttpStatusCode.BadRequest,
                    HttpStatusCode.NotFound,
                    HttpStatusCode.Conflict,
                    HttpStatusCode.UnprocessableEntity
                ],
                _ => null
            };
        }

        ConsistencyGuard.ThrowIf(statusCodes == null);
        return statusCodes;
    }

    private void SetNonPrimaryResponseMetadata(ActionDescriptor descriptor, BuiltinJsonApiActionMethod actionMethod, ResourceType? resourceType,
        Dictionary<RelationshipAttribute, ActionDescriptor> descriptorsByRelationship, RelationshipAttribute relationship, Type? documentType)
    {
        ConsistencyGuard.ThrowIf(descriptor.AttributeRouteInfo == null);

        if (!descriptorsByRelationship.TryGetValue(relationship, out ActionDescriptor? relationshipDescriptor))
        {
            relationshipDescriptor = Clone(descriptor);
            RemovePathParameter(relationshipDescriptor.Parameters, "relationshipName");
        }

        ExpandTemplate(relationshipDescriptor.AttributeRouteInfo!, relationship.PublicName);
        SetProduces(relationshipDescriptor, documentType);
        SetProducesResponseTypes(relationshipDescriptor, actionMethod, resourceType, documentType);

        descriptorsByRelationship[relationship] = relationshipDescriptor;
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Hiding unsupported custom JSON:API action method [{HttpMethods}] {ActionMethod} in OpenAPI.")]
    private partial void LogSuppressedActionMethod(string httpMethods, string? actionMethod);
}
