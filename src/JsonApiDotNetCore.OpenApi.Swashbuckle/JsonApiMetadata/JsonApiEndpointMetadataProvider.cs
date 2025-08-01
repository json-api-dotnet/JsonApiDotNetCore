using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata.ActionMethods;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata.Documents;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Documents;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata;

/// <summary>
/// Provides JsonApiDotNetCore related metadata for an ASP.NET action method that can only be computed from the <see cref="ResourceGraph" /> at runtime.
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

    public JsonApiEndpointMetadata Get(ActionDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        var actionMethod = OpenApiActionMethod.Create(descriptor);
        JsonApiEndpointMetadata? metadata = null;

        switch (actionMethod)
        {
            case AtomicOperationsActionMethod:
            {
                metadata = new JsonApiEndpointMetadata(AtomicOperationsRequestMetadata.Instance, AtomicOperationsResponseMetadata.Instance);
                break;
            }
            case JsonApiActionMethod jsonApiActionMethod:
            {
                ResourceType? primaryResourceType = _controllerResourceMapping.GetResourceTypeForController(jsonApiActionMethod.ControllerType);
                ConsistencyGuard.ThrowIf(primaryResourceType == null);

                IJsonApiRequestMetadata? requestMetadata = GetRequestMetadata(jsonApiActionMethod.Endpoint, primaryResourceType);
                IJsonApiResponseMetadata? responseMetadata = GetResponseMetadata(jsonApiActionMethod.Endpoint, primaryResourceType);
                metadata = new JsonApiEndpointMetadata(requestMetadata, responseMetadata);
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

    private IJsonApiResponseMetadata? GetResponseMetadata(JsonApiEndpoints endpoint, ResourceType primaryResourceType)
    {
        return endpoint switch
        {
            JsonApiEndpoints.GetCollection or JsonApiEndpoints.GetSingle or JsonApiEndpoints.Post or JsonApiEndpoints.Patch => GetPrimaryResponseMetadata(
                primaryResourceType.ClrType, endpoint == JsonApiEndpoints.GetCollection),
            JsonApiEndpoints.Delete => GetEmptyPrimaryResponseMetadata(),
            JsonApiEndpoints.GetSecondary => GetSecondaryResponseMetadata(primaryResourceType.Relationships),
            JsonApiEndpoints.GetRelationship => GetRelationshipResponseMetadata(primaryResourceType.Relationships),
            JsonApiEndpoints.PostRelationship or JsonApiEndpoints.PatchRelationship or JsonApiEndpoints.DeleteRelationship =>
                GetEmptyRelationshipResponseMetadata(primaryResourceType.Relationships, endpoint != JsonApiEndpoints.PatchRelationship),
            _ => null
        };
    }

    private static PrimaryResponseMetadata GetEmptyPrimaryResponseMetadata()
    {
        return new PrimaryResponseMetadata(null);
    }

    private static PrimaryResponseMetadata GetPrimaryResponseMetadata(Type resourceClrType, bool endpointReturnsCollection)
    {
        Type documentOpenType = endpointReturnsCollection ? typeof(CollectionResponseDocument<>) : typeof(PrimaryResponseDocument<>);
        Type documentType = documentOpenType.MakeGenericType(resourceClrType);

        return new PrimaryResponseMetadata(documentType);
    }

    private SecondaryResponseMetadata GetSecondaryResponseMetadata(IEnumerable<RelationshipAttribute> relationships)
    {
        Dictionary<RelationshipAttribute, Type> documentTypesByRelationship = relationships.ToDictionary(relationship => relationship,
            _nonPrimaryDocumentTypeFactory.GetForSecondaryResponse);

        return new SecondaryResponseMetadata(documentTypesByRelationship.AsReadOnly());
    }

    private RelationshipResponseMetadata GetRelationshipResponseMetadata(IReadOnlyCollection<RelationshipAttribute> relationships)
    {
        Dictionary<RelationshipAttribute, Type> documentTypesByRelationship = relationships.ToDictionary(relationship => relationship,
            _nonPrimaryDocumentTypeFactory.GetForRelationshipResponse);

        return new RelationshipResponseMetadata(documentTypesByRelationship.AsReadOnly());
    }

    private static EmptyRelationshipResponseMetadata GetEmptyRelationshipResponseMetadata(IReadOnlyCollection<RelationshipAttribute> relationships,
        bool ignoreHasOneRelationships)
    {
        IReadOnlyCollection<RelationshipAttribute> relationshipsOfEndpoint =
            ignoreHasOneRelationships ? relationships.OfType<HasManyAttribute>().ToList().AsReadOnly() : relationships;

        return new EmptyRelationshipResponseMetadata(relationshipsOfEndpoint);
    }
}
