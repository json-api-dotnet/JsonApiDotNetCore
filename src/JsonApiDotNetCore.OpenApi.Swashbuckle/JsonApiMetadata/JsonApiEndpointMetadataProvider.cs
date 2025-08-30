using System.Collections.ObjectModel;
using System.Net;
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
/// Provides JSON:API metadata for an ASP.NET action method that is computed from the <see cref="ResourceGraph" />.
/// </summary>
internal sealed class JsonApiEndpointMetadataProvider
{
    private readonly IJsonApiOptions _options;
    private readonly IControllerResourceMapping _controllerResourceMapping;
    private readonly NonPrimaryDocumentTypeFactory _nonPrimaryDocumentTypeFactory;

    public JsonApiEndpointMetadataProvider(IJsonApiOptions options, IControllerResourceMapping controllerResourceMapping,
        NonPrimaryDocumentTypeFactory nonPrimaryDocumentTypeFactory)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(controllerResourceMapping);
        ArgumentNullException.ThrowIfNull(nonPrimaryDocumentTypeFactory);

        _options = options;
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
            case CustomResourceActionMethod customJsonApiActionMethod:
            {
                // TODO: Apply conventions (hard-coded to unblock test, for now).

                ResourceType? primaryResourceType = _controllerResourceMapping.GetResourceTypeForController(customJsonApiActionMethod.ControllerType);
                ConsistencyGuard.ThrowIf(primaryResourceType == null);

                ReadOnlyCollection<HttpStatusCode> successStatusCodes = new List<HttpStatusCode>([HttpStatusCode.OK]).AsReadOnly();
                ReadOnlyCollection<HttpStatusCode> errorStatusCodes = new List<HttpStatusCode>([HttpStatusCode.NotFound]).AsReadOnly();

                metadata = new JsonApiEndpointMetadata(null,
                    GetPrimaryResponseMetadata(primaryResourceType.ClrType, false, successStatusCodes, errorStatusCodes));

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
            JsonApiEndpoints.GetRelationship => GetRelationshipResponseMetadata(primaryResourceType.Relationships, successStatusCodes, errorStatusCodes),
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
        ReadOnlyCollection<HttpStatusCode> successStatusCodes, ReadOnlyCollection<HttpStatusCode> errorStatusCodes)
    {
        Dictionary<RelationshipAttribute, Type> documentTypesByRelationship = relationships.ToDictionary(relationship => relationship,
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
}
