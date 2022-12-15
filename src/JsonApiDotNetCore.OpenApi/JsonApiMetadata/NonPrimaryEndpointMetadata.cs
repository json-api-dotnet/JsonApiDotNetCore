namespace JsonApiDotNetCore.OpenApi.JsonApiMetadata;

internal abstract class NonPrimaryEndpointMetadata
{
    public IDictionary<string, Type> DocumentTypesByRelationshipName { get; }

    protected NonPrimaryEndpointMetadata(IDictionary<string, Type> documentTypesByRelationshipName)
    {
        ArgumentGuard.NotNull(documentTypesByRelationshipName);

        DocumentTypesByRelationshipName = documentTypesByRelationshipName;
    }
}
