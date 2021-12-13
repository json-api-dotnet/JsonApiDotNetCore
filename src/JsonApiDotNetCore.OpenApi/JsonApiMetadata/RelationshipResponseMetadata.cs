namespace JsonApiDotNetCore.OpenApi.JsonApiMetadata;

internal sealed class RelationshipResponseMetadata : NonPrimaryEndpointMetadata, IJsonApiResponseMetadata
{
    public RelationshipResponseMetadata(IDictionary<string, Type> documentTypesByRelationshipName)
        : base(documentTypesByRelationshipName)
    {
    }
}
