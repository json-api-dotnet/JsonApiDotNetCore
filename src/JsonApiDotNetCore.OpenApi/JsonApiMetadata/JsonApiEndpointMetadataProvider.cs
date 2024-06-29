using System.Reflection;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Documents;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.OpenApi.JsonApiMetadata;

/// <summary>
/// Provides JsonApiDotNetCore related metadata for an ASP.NET controller action that can only be computed from the <see cref="ResourceGraph" /> at
/// runtime.
/// </summary>
internal sealed class JsonApiEndpointMetadataProvider
{
    private readonly EndpointResolver _endpointResolver;
    private readonly IControllerResourceMapping _controllerResourceMapping;
    private readonly NonPrimaryDocumentTypeFactory _nonPrimaryDocumentTypeFactory;

    public JsonApiEndpointMetadataProvider(EndpointResolver endpointResolver, IControllerResourceMapping controllerResourceMapping,
        NonPrimaryDocumentTypeFactory nonPrimaryDocumentTypeFactory)
    {
        ArgumentGuard.NotNull(endpointResolver);
        ArgumentGuard.NotNull(controllerResourceMapping);
        ArgumentGuard.NotNull(nonPrimaryDocumentTypeFactory);

        _endpointResolver = endpointResolver;
        _controllerResourceMapping = controllerResourceMapping;
        _nonPrimaryDocumentTypeFactory = nonPrimaryDocumentTypeFactory;
    }

    public JsonApiEndpointMetadataContainer Get(MethodInfo controllerAction)
    {
        ArgumentGuard.NotNull(controllerAction);

        JsonApiEndpoint? endpoint = _endpointResolver.Get(controllerAction);

        if (endpoint == null)
        {
            throw new NotSupportedException($"Unable to provide metadata for non-JSON:API endpoint '{controllerAction.ReflectedType!.FullName}'.");
        }

        if (endpoint == JsonApiEndpoint.PostOperations)
        {
            return new JsonApiEndpointMetadataContainer(AtomicOperationsRequestMetadata.Instance, AtomicOperationsResponseMetadata.Instance);
        }

        ResourceType? primaryResourceType = _controllerResourceMapping.GetResourceTypeForController(controllerAction.ReflectedType);

        if (primaryResourceType == null)
        {
            throw new UnreachableCodeException();
        }

        IJsonApiRequestMetadata? requestMetadata = GetRequestMetadata(endpoint.Value, primaryResourceType);
        IJsonApiResponseMetadata? responseMetadata = GetResponseMetadata(endpoint.Value, primaryResourceType);
        return new JsonApiEndpointMetadataContainer(requestMetadata, responseMetadata);
    }

    private IJsonApiRequestMetadata? GetRequestMetadata(JsonApiEndpoint endpoint, ResourceType primaryResourceType)
    {
        return endpoint switch
        {
            JsonApiEndpoint.PostResource => GetPostResourceRequestMetadata(primaryResourceType.ClrType),
            JsonApiEndpoint.PatchResource => GetPatchResourceRequestMetadata(primaryResourceType.ClrType),
            JsonApiEndpoint.PostRelationship or JsonApiEndpoint.PatchRelationship or JsonApiEndpoint.DeleteRelationship => GetRelationshipRequestMetadata(
                primaryResourceType.Relationships, endpoint != JsonApiEndpoint.PatchRelationship),
            _ => null
        };
    }

    private static PrimaryRequestMetadata GetPostResourceRequestMetadata(Type resourceClrType)
    {
        Type documentType = typeof(CreateResourceRequestDocument<>).MakeGenericType(resourceClrType);

        return new PrimaryRequestMetadata(documentType);
    }

    private static PrimaryRequestMetadata GetPatchResourceRequestMetadata(Type resourceClrType)
    {
        Type documentType = typeof(UpdateResourceRequestDocument<>).MakeGenericType(resourceClrType);

        return new PrimaryRequestMetadata(documentType);
    }

    private RelationshipRequestMetadata GetRelationshipRequestMetadata(IEnumerable<RelationshipAttribute> relationships, bool ignoreHasOneRelationships)
    {
        IEnumerable<RelationshipAttribute> relationshipsOfEndpoint = ignoreHasOneRelationships ? relationships.OfType<HasManyAttribute>() : relationships;

        IDictionary<string, Type> requestDocumentTypesByRelationshipName = relationshipsOfEndpoint.ToDictionary(relationship => relationship.PublicName,
            _nonPrimaryDocumentTypeFactory.GetForRelationshipRequest);

        return new RelationshipRequestMetadata(requestDocumentTypesByRelationshipName);
    }

    private IJsonApiResponseMetadata? GetResponseMetadata(JsonApiEndpoint endpoint, ResourceType primaryResourceType)
    {
        return endpoint switch
        {
            JsonApiEndpoint.GetCollection or JsonApiEndpoint.GetSingle or JsonApiEndpoint.PostResource or JsonApiEndpoint.PatchResource =>
                GetPrimaryResponseMetadata(primaryResourceType.ClrType, endpoint == JsonApiEndpoint.GetCollection),
            JsonApiEndpoint.GetSecondary => GetSecondaryResponseMetadata(primaryResourceType.Relationships),
            JsonApiEndpoint.GetRelationship => GetRelationshipResponseMetadata(primaryResourceType.Relationships),
            _ => null
        };
    }

    private static PrimaryResponseMetadata GetPrimaryResponseMetadata(Type resourceClrType, bool endpointReturnsCollection)
    {
        Type documentOpenType = endpointReturnsCollection ? typeof(ResourceCollectionResponseDocument<>) : typeof(PrimaryResourceResponseDocument<>);
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
