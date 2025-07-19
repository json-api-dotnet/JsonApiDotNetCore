namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata;

internal sealed class RelationshipResponseMetadata(IDictionary<string, Type> documentTypesByRelationshipName)
    : NonPrimaryEndpointMetadata(documentTypesByRelationshipName), IJsonApiResponseMetadata;
