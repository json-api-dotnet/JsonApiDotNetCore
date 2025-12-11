using System.Collections.ObjectModel;
using System.Net;
using System.Reflection;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata.ActionMethods;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata.Documents;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Documents;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata;

/// <summary>
/// Provides JSON:API metadata for an ASP.NET action method that is computed from the <see cref="ResourceGraph" />.
/// </summary>
internal sealed class JsonApiEndpointMetadataProvider
{
    private readonly IJsonApiOptions _options;
    private readonly IResourceGraph _resourceGraph;
    private readonly IControllerResourceMapping _controllerResourceMapping;
    private readonly NonPrimaryDocumentTypeFactory _nonPrimaryDocumentTypeFactory;

    public JsonApiEndpointMetadataProvider(IJsonApiOptions options, IResourceGraph resourceGraph, IControllerResourceMapping controllerResourceMapping,
        NonPrimaryDocumentTypeFactory nonPrimaryDocumentTypeFactory)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(resourceGraph);
        ArgumentNullException.ThrowIfNull(controllerResourceMapping);
        ArgumentNullException.ThrowIfNull(nonPrimaryDocumentTypeFactory);

        _options = options;
        _resourceGraph = resourceGraph;
        _controllerResourceMapping = controllerResourceMapping;
        _nonPrimaryDocumentTypeFactory = nonPrimaryDocumentTypeFactory;
    }

    public JsonApiEndpointMetadata Get(ActionDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        var actionMethod = JsonApiActionMethod.TryCreate(descriptor);
        JsonApiEndpointMetadata? metadata = null;

        switch (actionMethod)
        {
            case OperationsActionMethod:
            {
                metadata = new JsonApiEndpointMetadata(AtomicOperationsRequestMetadata.Instance, AtomicOperationsResponseMetadata.Instance);
                break;
            }
            case BuiltinResourceActionMethod builtinJsonApiActionMethod:
            {
                JsonApiEndpoints endpoint = builtinJsonApiActionMethod.Endpoint;

                ResourceType? resourceType = _controllerResourceMapping.GetResourceTypeForController(builtinJsonApiActionMethod.ControllerType);
                ConsistencyGuard.ThrowIf(resourceType == null);

                ReadOnlyCollection<HttpStatusCode> successStatusCodes = GetSuccessStatusCodesForActionMethod(builtinJsonApiActionMethod);
                ReadOnlyCollection<HttpStatusCode> errorStatusCodes = GetErrorStatusCodesForActionMethod(builtinJsonApiActionMethod, resourceType);

                IJsonApiRequestMetadata? requestMetadata = GetRequestMetadata(endpoint, resourceType);
                IJsonApiResponseMetadata? responseMetadata = GetResponseMetadata(endpoint, resourceType, successStatusCodes, errorStatusCodes);

                metadata = new JsonApiEndpointMetadata(requestMetadata, responseMetadata);
                break;
            }
            case CustomResourceActionMethod customResourceActionMethod:
            {
                ResourceType? resourceType = _controllerResourceMapping.GetResourceTypeForController(customResourceActionMethod.ControllerType);
                ConsistencyGuard.ThrowIf(resourceType == null);

                metadata = GetCustomMetadata(customResourceActionMethod.Descriptor, resourceType);
                break;
            }
        }

        ConsistencyGuard.ThrowIf(metadata == null);
        return metadata;
    }

    private IJsonApiRequestMetadata? GetRequestMetadata(JsonApiEndpoints endpoint, ResourceType primaryResourceType)
    {
        return endpoint switch
        {
            JsonApiEndpoints.Post => GetPostResourceRequestMetadata(primaryResourceType.ClrType),
            JsonApiEndpoints.Patch => GetPatchResourceRequestMetadata(primaryResourceType.ClrType),
            JsonApiEndpoints.PostRelationship or JsonApiEndpoints.PatchRelationship or JsonApiEndpoints.DeleteRelationship => GetRelationshipRequestMetadata(
                primaryResourceType.Relationships, endpoint != JsonApiEndpoints.PatchRelationship),
            _ => null
        };
    }

    private static PrimaryRequestMetadata GetPostResourceRequestMetadata(Type resourceClrType)
    {
        Type documentType = typeof(CreateRequestDocument<>).MakeGenericType(resourceClrType);

        return new PrimaryRequestMetadata(documentType);
    }

    private static PrimaryRequestMetadata GetPatchResourceRequestMetadata(Type resourceClrType)
    {
        Type documentType = typeof(UpdateRequestDocument<>).MakeGenericType(resourceClrType);

        return new PrimaryRequestMetadata(documentType);
    }

    private RelationshipRequestMetadata GetRelationshipRequestMetadata(IReadOnlyCollection<RelationshipAttribute> relationships, bool ignoreHasOneRelationships)
    {
        IEnumerable<RelationshipAttribute> relationshipsOfEndpoint = ignoreHasOneRelationships ? relationships.OfType<HasManyAttribute>() : relationships;

        Dictionary<RelationshipAttribute, Type> documentTypesByRelationship = relationshipsOfEndpoint.ToDictionary(relationship => relationship,
            _nonPrimaryDocumentTypeFactory.GetForRelationshipRequest);

        return new RelationshipRequestMetadata(documentTypesByRelationship.AsReadOnly());
    }

    private IJsonApiResponseMetadata? GetResponseMetadata(JsonApiEndpoints endpoint, ResourceType primaryResourceType,
        ReadOnlyCollection<HttpStatusCode> successStatusCodes, ReadOnlyCollection<HttpStatusCode> errorStatusCodes)
    {
        return endpoint switch
        {
            JsonApiEndpoints.GetCollection => GetPrimaryResponseMetadata(primaryResourceType.ClrType, true, successStatusCodes, errorStatusCodes),
            JsonApiEndpoints.GetSingle or JsonApiEndpoints.Post or JsonApiEndpoints.Patch => GetPrimaryResponseMetadata(primaryResourceType.ClrType, false,
                successStatusCodes, errorStatusCodes),
            JsonApiEndpoints.Delete => GetEmptyPrimaryResponseMetadata(successStatusCodes, errorStatusCodes),
            JsonApiEndpoints.GetSecondary => GetSecondaryResponseMetadata(primaryResourceType.Relationships, successStatusCodes, errorStatusCodes),
            JsonApiEndpoints.GetRelationship => GetRelationshipResponseMetadata(primaryResourceType.Relationships, false, successStatusCodes, errorStatusCodes),
            JsonApiEndpoints.PatchRelationship => GetEmptyRelationshipResponseMetadata(primaryResourceType.Relationships, false, successStatusCodes,
                errorStatusCodes),
            JsonApiEndpoints.PostRelationship or JsonApiEndpoints.DeleteRelationship => GetEmptyRelationshipResponseMetadata(primaryResourceType.Relationships,
                true, successStatusCodes, errorStatusCodes),
            _ => null
        };
    }

    private static ReadOnlyCollection<HttpStatusCode> GetSuccessStatusCodesForActionMethod(BuiltinResourceActionMethod actionMethod)
    {
        HttpStatusCode[]? statusCodes = actionMethod.Endpoint switch
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

        ConsistencyGuard.ThrowIf(statusCodes == null);
        return statusCodes.AsReadOnly();
    }

    private ReadOnlyCollection<HttpStatusCode> GetErrorStatusCodesForActionMethod(BuiltinResourceActionMethod actionMethod, ResourceType resourceType)
    {
        // This condition doesn't apply to atomic operations, because Forbidden is also used when an operation is not accessible.
        ClientIdGenerationMode clientIdGeneration = resourceType.ClientIdGeneration ?? _options.ClientIdGeneration;

        HttpStatusCode[]? statusCodes = actionMethod.Endpoint switch
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

        ConsistencyGuard.ThrowIf(statusCodes == null);
        return statusCodes.AsReadOnly();
    }

    private static PrimaryResponseMetadata GetEmptyPrimaryResponseMetadata(ReadOnlyCollection<HttpStatusCode> successStatusCodes,
        ReadOnlyCollection<HttpStatusCode> errorStatusCodes)
    {
        return new PrimaryResponseMetadata(null, successStatusCodes, errorStatusCodes);
    }

    private static PrimaryResponseMetadata GetPrimaryResponseMetadata(Type resourceClrType, bool endpointReturnsCollection,
        ReadOnlyCollection<HttpStatusCode> successStatusCodes, ReadOnlyCollection<HttpStatusCode> errorStatusCodes)
    {
        Type documentOpenType = endpointReturnsCollection ? typeof(CollectionResponseDocument<>) : typeof(PrimaryResponseDocument<>);
        Type documentType = documentOpenType.MakeGenericType(resourceClrType);

        return new PrimaryResponseMetadata(documentType, successStatusCodes, errorStatusCodes);
    }

    private SecondaryResponseMetadata GetSecondaryResponseMetadata(IEnumerable<RelationshipAttribute> relationships,
        ReadOnlyCollection<HttpStatusCode> successStatusCodes, ReadOnlyCollection<HttpStatusCode> errorStatusCodes)
    {
        Dictionary<RelationshipAttribute, Type> documentTypesByRelationship = relationships.ToDictionary(relationship => relationship,
            _nonPrimaryDocumentTypeFactory.GetForSecondaryResponse);

        return new SecondaryResponseMetadata(documentTypesByRelationship.AsReadOnly(), successStatusCodes, errorStatusCodes);
    }

    private RelationshipResponseMetadata GetRelationshipResponseMetadata(IReadOnlyCollection<RelationshipAttribute> relationships,
        bool ignoreHasOneRelationships, ReadOnlyCollection<HttpStatusCode> successStatusCodes, ReadOnlyCollection<HttpStatusCode> errorStatusCodes)
    {
        IReadOnlyCollection<RelationshipAttribute> relationshipsOfEndpoint =
            ignoreHasOneRelationships ? relationships.OfType<HasManyAttribute>().ToList().AsReadOnly() : relationships;

        Dictionary<RelationshipAttribute, Type> documentTypesByRelationship = relationshipsOfEndpoint.ToDictionary(relationship => relationship,
            _nonPrimaryDocumentTypeFactory.GetForRelationshipResponse);

        return new RelationshipResponseMetadata(documentTypesByRelationship.AsReadOnly(), successStatusCodes, errorStatusCodes);
    }

    private static EmptyRelationshipResponseMetadata GetEmptyRelationshipResponseMetadata(IReadOnlyCollection<RelationshipAttribute> relationships,
        bool ignoreHasOneRelationships, ReadOnlyCollection<HttpStatusCode> successStatusCodes, ReadOnlyCollection<HttpStatusCode> errorStatusCodes)
    {
        IReadOnlyCollection<RelationshipAttribute> relationshipsOfEndpoint =
            ignoreHasOneRelationships ? relationships.OfType<HasManyAttribute>().ToList().AsReadOnly() : relationships;

        return new EmptyRelationshipResponseMetadata(relationshipsOfEndpoint, successStatusCodes, errorStatusCodes);
    }

    private JsonApiEndpointMetadata GetCustomMetadata(ActionDescriptor descriptor, ResourceType controllerResourceType)
    {
        // The heuristics used here are kinda arbitrary, because we can't really know the purpose of a custom action method, while we do need
        // to choose JSON:API request/response types. Please open an issue describing your action method signature(s) if this doesn't meet your needs.

        bool hasParameterForId = false;
        bool hasParameterForRelationshipName = false;
        bool hasRelationshipsInRoute = false;

        if (descriptor is ControllerActionDescriptor { AttributeRouteInfo.Template: not null } controllerActionDescriptor)
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            // Justification: Bug in the C# compiler, similar to https://github.com/dotnet/roslyn/issues/50162.
            hasParameterForId = controllerActionDescriptor.AttributeRouteInfo.Template.Contains("{id}");
            hasParameterForRelationshipName = controllerActionDescriptor.AttributeRouteInfo.Template.Contains("{relationshipName}");
            hasRelationshipsInRoute = controllerActionDescriptor.AttributeRouteInfo.Template.Split('/').Contains("relationships");
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        MethodInfo? actionMethod = descriptor.TryGetActionMethod();
        ConsistencyGuard.ThrowIf(actionMethod == null);

        HashSet<string> httpMethods = actionMethod.GetCustomAttributes<HttpMethodAttribute>(true).SelectMany(httpMethod => httpMethod.HttpMethods).ToHashSet();
        bool skipHasOneAtRelationshipEndpoint = httpMethods.Contains("POST") || httpMethods.Contains("DELETE");

        IJsonApiRequestMetadata? requestMetadata = GetCustomRequestMetadata(descriptor, controllerResourceType, hasParameterForId,
            hasParameterForRelationshipName, skipHasOneAtRelationshipEndpoint);

        IJsonApiResponseMetadata? responseMetadata = GetCustomResponseMetadata(descriptor, controllerResourceType, hasParameterForRelationshipName,
            hasRelationshipsInRoute, skipHasOneAtRelationshipEndpoint);

        return new JsonApiEndpointMetadata(requestMetadata, responseMetadata);
    }

    private IJsonApiRequestMetadata? GetCustomRequestMetadata(ActionDescriptor descriptor, ResourceType controllerResourceType, bool hasParameterForId,
        bool hasParameterForRelationshipName, bool skipHasOneAtRelationshipEndpoint)
    {
        ConsumesAttribute? consumes = descriptor.FilterDescriptors.Select(filter => filter.Filter).OfType<ConsumesAttribute>().FirstOrDefault();

        if (consumes != null)
        {
            Type? endpointResourceClrType = ((IAcceptsMetadata)consumes).RequestType;
            ResourceType? endpointResourceType = endpointResourceClrType != null ? _resourceGraph.GetResourceType(endpointResourceClrType) : null;
            ResourceType primaryResourceType = endpointResourceType ?? controllerResourceType;

            if (!hasParameterForRelationshipName)
            {
                return hasParameterForId
                    ? GetPatchResourceRequestMetadata(primaryResourceType.ClrType)
                    : GetPostResourceRequestMetadata(primaryResourceType.ClrType);
            }

            return GetRelationshipRequestMetadata(primaryResourceType.Relationships, skipHasOneAtRelationshipEndpoint);
        }

        return null;
    }

    private IJsonApiResponseMetadata? GetCustomResponseMetadata(ActionDescriptor descriptor, ResourceType controllerResourceType,
        bool hasParameterForRelationshipName, bool hasRelationshipsInRoute, bool skipHasOneAtRelationshipEndpoint)
    {
        ResourceType? successResponseBodyType = null;
        bool isResponseBodyCollection = false;
        HashSet<HttpStatusCode> successStatusCodeSet = [];
        HashSet<HttpStatusCode> errorStatusCodeSet = [];

        foreach (ProducesResponseTypeAttribute produces in descriptor.FilterDescriptors.Select(filter => filter.Filter).OfType<ProducesResponseTypeAttribute>())
        {
            bool isSuccessResponse = produces.StatusCode < 400;

            if (isSuccessResponse && produces.Type != typeof(void))
            {
                Type? elementType = CollectionConverter.Instance.FindCollectionElementType(produces.Type);
                successResponseBodyType = _resourceGraph.GetResourceType(elementType ?? produces.Type);
                isResponseBodyCollection = elementType != null;
            }

            if (isSuccessResponse)
            {
                successStatusCodeSet.Add((HttpStatusCode)produces.StatusCode);
            }
            else
            {
                errorStatusCodeSet.Add((HttpStatusCode)produces.StatusCode);
            }
        }

        ResourceType primaryResourceType = successResponseBodyType ?? controllerResourceType;
        ReadOnlyCollection<HttpStatusCode> successStatusCodes = successStatusCodeSet.ToArray().AsReadOnly();
        ReadOnlyCollection<HttpStatusCode> errorStatusCodes = errorStatusCodeSet.ToArray().AsReadOnly();

        if (!hasParameterForRelationshipName && !hasRelationshipsInRoute)
        {
            return successResponseBodyType != null
                ? GetPrimaryResponseMetadata(successResponseBodyType.ClrType, isResponseBodyCollection, successStatusCodes, errorStatusCodes)
                : GetEmptyPrimaryResponseMetadata(successStatusCodes, errorStatusCodes);
        }

        if (hasParameterForRelationshipName && !hasRelationshipsInRoute)
        {
            return GetSecondaryResponseMetadata(primaryResourceType.Relationships, successStatusCodes, errorStatusCodes);
        }

        if (hasParameterForRelationshipName && hasRelationshipsInRoute)
        {
            return successResponseBodyType != null
                ? GetRelationshipResponseMetadata(primaryResourceType.Relationships, skipHasOneAtRelationshipEndpoint, successStatusCodes, errorStatusCodes)
                : GetEmptyRelationshipResponseMetadata(primaryResourceType.Relationships, skipHasOneAtRelationshipEndpoint, successStatusCodes,
                    errorStatusCodes);
        }

        return null;
    }
}
