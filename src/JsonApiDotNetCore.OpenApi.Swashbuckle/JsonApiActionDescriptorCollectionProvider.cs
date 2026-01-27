using System.Collections.Concurrent;
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

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

/// <summary>
/// Adds OpenAPI metadata (consumes, produces) to <see cref="ActionDescriptor" />s and performs endpoint expansion for secondary and relationship
/// endpoints. For example: <code><![CDATA[
/// /article/{id}/{relationshipName} -> /article/{id}/author, /article/{id}/revisions, etc.
/// ]]></code>
/// </summary>
internal sealed class JsonApiActionDescriptorCollectionProvider : IActionDescriptorCollectionProvider
{
    private const int FilterScope = 10;
    private static readonly Type ErrorDocumentType = typeof(ErrorResponseDocument);

    private readonly IActionDescriptorCollectionProvider _defaultProvider;
    private readonly IControllerResourceMapping _controllerResourceMapping;
    private readonly JsonApiEndpointMetadataProvider _jsonApiEndpointMetadataProvider;
    private readonly ConcurrentDictionary<int, Lazy<ActionDescriptorCollection>> _versionedActionDescriptorCache = new();

    public ActionDescriptorCollection ActionDescriptors =>
        _versionedActionDescriptorCache.GetOrAdd(_defaultProvider.ActionDescriptors.Version, LazyGetActionDescriptors).Value;

    public JsonApiActionDescriptorCollectionProvider(IActionDescriptorCollectionProvider defaultProvider, IControllerResourceMapping controllerResourceMapping,
        JsonApiEndpointMetadataProvider jsonApiEndpointMetadataProvider)
    {
        ArgumentNullException.ThrowIfNull(defaultProvider);
        ArgumentNullException.ThrowIfNull(controllerResourceMapping);
        ArgumentNullException.ThrowIfNull(jsonApiEndpointMetadataProvider);

        _defaultProvider = defaultProvider;
        _controllerResourceMapping = controllerResourceMapping;
        _jsonApiEndpointMetadataProvider = jsonApiEndpointMetadataProvider;
    }

    private Lazy<ActionDescriptorCollection> LazyGetActionDescriptors(int version)
    {
        // https://andrewlock.net/making-getoradd-on-concurrentdictionary-thread-safe-using-lazy/
        return new Lazy<ActionDescriptorCollection>(() => GetActionDescriptors(version), LazyThreadSafetyMode.ExecutionAndPublication);
    }

    private ActionDescriptorCollection GetActionDescriptors(int version)
    {
        List<ActionDescriptor> descriptors = [];

        foreach (ActionDescriptor descriptor in _defaultProvider.ActionDescriptors.Items)
        {
            if (!descriptor.EndpointMetadata.OfType<IHttpMethodMetadata>().SelectMany(metadata => metadata.HttpMethods).Any())
            {
                // Technically incorrect: when no verbs, the endpoint is exposed on all verbs. But Swashbuckle hides it anyway.
                continue;
            }

            var actionMethod = JsonApiActionMethod.TryCreate(descriptor);

            if (actionMethod != null)
            {
                if (!IsVisibleEndpoint(descriptor))
                {
                    continue;
                }

                ResourceType? resourceType = _controllerResourceMapping.GetResourceTypeForController(actionMethod.ControllerType);

                if (actionMethod is BuiltinResourceActionMethod builtinResourceActionMethod)
                {
                    ConsistencyGuard.ThrowIf(resourceType == null);

                    if (ShouldSuppressEndpoint(builtinResourceActionMethod.Endpoint, resourceType))
                    {
                        continue;
                    }
                }

                ActionDescriptor[] replacementDescriptors = SetEndpointMetadata(descriptor);
                descriptors.AddRange(replacementDescriptors);

                continue;
            }

            descriptors.Add(descriptor);
        }

        return new ActionDescriptorCollection(descriptors.AsReadOnly(), version);
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

    private ActionDescriptor[] SetEndpointMetadata(ActionDescriptor descriptor)
    {
        Dictionary<RelationshipAttribute, ActionDescriptor> descriptorsByRelationship = [];
        bool isNonPrimaryEndpoint = false;

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
                isNonPrimaryEndpoint = true;

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

                SetProducesResponseTypes(descriptor, atomicOperationsResponseMetadata.DocumentType, atomicOperationsResponseMetadata.SuccessStatusCodes,
                    atomicOperationsResponseMetadata.ErrorStatusCodes);

                break;
            }
            case PrimaryResponseMetadata primaryResponseMetadata:
            {
                SetProduces(descriptor, primaryResponseMetadata.DocumentType);

                SetProducesResponseTypes(descriptor, primaryResponseMetadata.DocumentType, primaryResponseMetadata.SuccessStatusCodes,
                    primaryResponseMetadata.ErrorStatusCodes);

                break;
            }
            case NonPrimaryResponseMetadata nonPrimaryResponseMetadata:
            {
                isNonPrimaryEndpoint = true;

                foreach ((RelationshipAttribute relationship, Type documentType) in nonPrimaryResponseMetadata.DocumentTypesByRelationship)
                {
                    SetNonPrimaryResponseMetadata(descriptor, descriptorsByRelationship, relationship, documentType,
                        nonPrimaryResponseMetadata.SuccessStatusCodes, nonPrimaryResponseMetadata.ErrorStatusCodes);
                }

                break;
            }
            case EmptyRelationshipResponseMetadata emptyRelationshipResponseMetadata:
            {
                isNonPrimaryEndpoint = true;

                foreach (RelationshipAttribute relationship in emptyRelationshipResponseMetadata.Relationships)
                {
                    SetNonPrimaryResponseMetadata(descriptor, descriptorsByRelationship, relationship, null,
                        emptyRelationshipResponseMetadata.SuccessStatusCodes, emptyRelationshipResponseMetadata.ErrorStatusCodes);
                }

                break;
            }
        }

        return isNonPrimaryEndpoint ? descriptorsByRelationship.Values.ToArray() : [descriptor];
    }

    private static void SetConsumes(ActionDescriptor descriptor, Type requestType, JsonApiMediaType mediaType)
    {
        RemoveFiltersForRequestBody(descriptor);

        // This value doesn't actually appear in the OpenAPI document, but is only used to invoke
        // JsonApiRequestFormatMetadataProvider.GetSupportedContentTypes(), which determines the actual request content type.
        string contentType = mediaType.ToString();

        if (descriptor is ControllerActionDescriptor controllerActionDescriptor &&
            controllerActionDescriptor.MethodInfo.GetCustomAttributes<ConsumesAttribute>().Any())
        {
            // A custom JSON:API action method with [Consumes] on it. Hide the attribute from Swashbuckle, so it uses our data in API Explorer.
            controllerActionDescriptor.MethodInfo = new MethodInfoWrapper(controllerActionDescriptor.MethodInfo, [typeof(ConsumesAttribute)]);
        }

        descriptor.FilterDescriptors.Add(new FilterDescriptor(new ConsumesAttribute(requestType, contentType), FilterScope));
    }

    private static void RemoveFiltersForRequestBody(ActionDescriptor descriptor)
    {
        // Custom action methods that take a request body are expected to be annotated with [Consumes].
        // We add the CLR type, so that an IIdentifiable type is lifted to a JSON:API type, which is why the existing annotation must be replaced.

        foreach (FilterDescriptor filterDescriptor in descriptor.FilterDescriptors.ToArray())
        {
            if (filterDescriptor.Filter is ConsumesAttribute)
            {
                descriptor.FilterDescriptors.Remove(filterDescriptor);
            }
        }
    }

    private static void UpdateRequestBodyParameterDescriptor(ActionDescriptor descriptor, Type documentType, string? parameterName)
    {
        ControllerParameterDescriptor? requestBodyDescriptor = descriptor.GetBodyParameterDescriptor();

        if (requestBodyDescriptor == null)
        {
            MethodInfo? actionMethod = descriptor.TryGetActionMethod();
            ConsistencyGuard.ThrowIf(actionMethod == null);

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

    private static void SetProduces(ActionDescriptor descriptor, Type? documentType)
    {
        IReadOnlyList<string> contentTypes = OpenApiContentTypeProvider.Instance.GetResponseContentTypes(documentType);

        if (contentTypes.Count > 0)
        {
            descriptor.FilterDescriptors.Add(new FilterDescriptor(new ProducesAttribute(contentTypes[0]), FilterScope));
        }
    }

    private static void SetProducesResponseTypes(ActionDescriptor descriptor, Type? documentType, IReadOnlyCollection<HttpStatusCode> successStatusCodes,
        IReadOnlyCollection<HttpStatusCode> errorStatusCodes)
    {
        foreach (HttpStatusCode statusCode in successStatusCodes.Order())
        {
            RemoveFiltersForStatusCode(descriptor, statusCode);

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

        foreach (HttpStatusCode statusCode in errorStatusCodes.Order())
        {
            RemoveFiltersForStatusCode(descriptor, statusCode);

            descriptor.FilterDescriptors.Add(errorContentType != null
                ? new FilterDescriptor(new ProducesResponseTypeAttribute(ErrorDocumentType, (int)statusCode, errorContentType), FilterScope)
                : new FilterDescriptor(new ProducesResponseTypeAttribute(ErrorDocumentType, (int)statusCode), FilterScope));
        }
    }

    private static void RemoveFiltersForStatusCode(ActionDescriptor descriptor, HttpStatusCode statusCode)
    {
        // Custom action methods are expected to be annotated with [ProducesResponseType] to express (1) the return type(s) on success and
        // (2) possible error status codes. We add the CLR types, so that IIdentifiable types are lifted to JSON:API types, which is why
        // the existing annotations must be replaced.

        foreach (FilterDescriptor filterDescriptor in descriptor.FilterDescriptors.ToArray())
        {
            if (filterDescriptor.Filter is ProducesResponseTypeAttribute produces && produces.StatusCode == (int)statusCode)
            {
                descriptor.FilterDescriptors.Remove(filterDescriptor);
            }
        }
    }

    private static bool StatusCodeHasNoResponseBody(HttpStatusCode statusCode)
    {
        int value = (int)statusCode;

        if (value < 200)
        {
            return true;
        }

        if (value is >= 300 and < 400)
        {
            return true;
        }

        return statusCode is HttpStatusCode.NoContent or HttpStatusCode.ResetContent;
    }

    private static void SetNonPrimaryResponseMetadata(ActionDescriptor descriptor,
        Dictionary<RelationshipAttribute, ActionDescriptor> descriptorsByRelationship, RelationshipAttribute relationship, Type? documentType,
        IReadOnlyCollection<HttpStatusCode> successStatusCodes, IReadOnlyCollection<HttpStatusCode> errorStatusCodes)
    {
        ConsistencyGuard.ThrowIf(descriptor.AttributeRouteInfo == null);

        if (!descriptorsByRelationship.TryGetValue(relationship, out ActionDescriptor? relationshipDescriptor))
        {
            relationshipDescriptor = Clone(descriptor);
            RemovePathParameter(relationshipDescriptor.Parameters, "relationshipName");
        }

        ExpandTemplate(relationshipDescriptor.AttributeRouteInfo!, relationship.PublicName);
        SetProduces(relationshipDescriptor, documentType);
        SetProducesResponseTypes(relationshipDescriptor, documentType, successStatusCodes, errorStatusCodes);

        descriptorsByRelationship[relationship] = relationshipDescriptor;
    }
}
