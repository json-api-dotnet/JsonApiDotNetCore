namespace JsonApiDotNetCore.OpenApi.JsonApiMetadata;

internal sealed class RelationshipRequestMetadata(IDictionary<string, Type> documentTypesByRelationshipName)
    : NonPrimaryEndpointMetadata(documentTypesByRelationshipName), IJsonApiRequestMetadata;
