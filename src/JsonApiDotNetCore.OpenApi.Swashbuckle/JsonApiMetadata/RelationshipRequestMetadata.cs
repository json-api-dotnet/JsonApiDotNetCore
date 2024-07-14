namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata;

internal sealed class RelationshipRequestMetadata(IDictionary<string, Type> documentTypesByRelationshipName)
    : NonPrimaryEndpointMetadata(documentTypesByRelationshipName), IJsonApiRequestMetadata;
