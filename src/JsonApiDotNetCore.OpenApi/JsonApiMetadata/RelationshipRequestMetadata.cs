namespace JsonApiDotNetCore.OpenApi.JsonApiMetadata;

internal sealed class RelationshipRequestMetadata : NonPrimaryEndpointMetadata, IJsonApiRequestMetadata
{
    public RelationshipRequestMetadata(IDictionary<string, Type> documentTypesByRelationshipName)
        : base(documentTypesByRelationshipName)
    {
    }
}
