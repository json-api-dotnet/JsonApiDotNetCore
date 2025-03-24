using System.Reflection;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Documents;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata;

/// <summary>
/// Provides JsonApiDotNetCore related metadata for an ASP.NET controller action that can only be computed from the <see cref="ResourceGraph" /> at
/// runtime.
/// </summary>
internal sealed class JsonApiEndpointMetadataProvider
{
    private readonly IControllerResourceMapping _controllerResourceMapping;
    private readonly NonPrimaryDocumentTypeFactory _nonPrimaryDocumentTypeFactory;

    public JsonApiEndpointMetadataProvider(IControllerResourceMapping controllerResourceMapping, NonPrimaryDocumentTypeFactory nonPrimaryDocumentTypeFactory)
    {
        ArgumentNullException.ThrowIfNull(controllerResourceMapping);
        ArgumentNullException.ThrowIfNull(nonPrimaryDocumentTypeFactory);

        _controllerResourceMapping = controllerResourceMapping;
        _nonPrimaryDocumentTypeFactory = nonPrimaryDocumentTypeFactory;
    }

    public JsonApiEndpointMetadataContainer Get(MethodInfo controllerAction)
    {
        ArgumentNullException.ThrowIfNull(controllerAction);

        if (EndpointResolver.Instance.IsAtomicOperationsController(controllerAction))
        {
            return new JsonApiEndpointMetadataContainer(AtomicOperationsRequestMetadata.Instance, AtomicOperationsResponseMetadata.Instance);
        }

        JsonApiEndpoints endpoint = EndpointResolver.Instance.GetEndpoint(controllerAction);

        if (endpoint == JsonApiEndpoints.None)
        {
            throw new NotSupportedException($"Unable to provide metadata for non-JSON:API endpoint '{controllerAction.ReflectedType!.FullName}'.");
        }

        ResourceType? primaryResourceType = _controllerResourceMapping.GetResourceTypeForController(controllerAction.ReflectedType);
        ConsistencyGuard.ThrowIf(primaryResourceType == null);

        IJsonApiRequestMetadata? requestMetadata = GetRequestMetadata(endpoint, primaryResourceType);
        IJsonApiResponseMetadata? responseMetadata = GetResponseMetadata(endpoint, primaryResourceType);
        return new JsonApiEndpointMetadataContainer(requestMetadata, responseMetadata);
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

    private RelationshipRequestMetadata GetRelationshipRequestMetadata(IEnumerable<RelationshipAttribute> relationships, bool ignoreHasOneRelationships)
    {
        IEnumerable<RelationshipAttribute> relationshipsOfEndpoint = ignoreHasOneRelationships ? relationships.OfType<HasManyAttribute>() : relationships;

        IDictionary<string, Type> requestDocumentTypesByRelationshipName = relationshipsOfEndpoint.ToDictionary(relationship => relationship.PublicName,
            _nonPrimaryDocumentTypeFactory.GetForRelationshipRequest);

        return new RelationshipRequestMetadata(requestDocumentTypesByRelationshipName);
    }

    private IJsonApiResponseMetadata? GetResponseMetadata(JsonApiEndpoints endpoint, ResourceType primaryResourceType)
    {
        return endpoint switch
        {
            JsonApiEndpoints.GetCollection or JsonApiEndpoints.GetSingle or JsonApiEndpoints.Post or JsonApiEndpoints.Patch => GetPrimaryResponseMetadata(
                primaryResourceType.ClrType, endpoint == JsonApiEndpoints.GetCollection),
            JsonApiEndpoints.GetSecondary => GetSecondaryResponseMetadata(primaryResourceType.Relationships),
            JsonApiEndpoints.GetRelationship => GetRelationshipResponseMetadata(primaryResourceType.Relationships),
            _ => null
        };
    }

    private static PrimaryResponseMetadata GetPrimaryResponseMetadata(Type resourceClrType, bool endpointReturnsCollection)
    {
        Type documentOpenType = endpointReturnsCollection ? typeof(CollectionResponseDocument<>) : typeof(PrimaryResponseDocument<>);
        Type documentType = documentOpenType.MakeGenericType(resourceClrType);

        return new PrimaryResponseMetadata(documentType);
    }

    private SecondaryResponseMetadata GetSecondaryResponseMetadata(IEnumerable<RelationshipAttribute> relationships)
    {
        IDictionary<string, Type> responseDocumentTypesByRelationshipName = relationships.ToDictionary(relationship => relationship.PublicName,
            _nonPrimaryDocumentTypeFactory.GetForSecondaryResponse);

        return new SecondaryResponseMetadata(responseDocumentTypesByRelationshipName);
    }

    private RelationshipResponseMetadata GetRelationshipResponseMetadata(IEnumerable<RelationshipAttribute> relationships)
    {
        IDictionary<string, Type> responseDocumentTypesByRelationshipName = relationships.ToDictionary(relationship => relationship.PublicName,
            _nonPrimaryDocumentTypeFactory.GetForRelationshipResponse);

        return new RelationshipResponseMetadata(responseDocumentTypesByRelationshipName);
    }
}
