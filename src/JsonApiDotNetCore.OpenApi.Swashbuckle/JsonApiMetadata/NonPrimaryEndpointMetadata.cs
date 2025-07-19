namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata;

internal abstract class NonPrimaryEndpointMetadata
{
    public IDictionary<string, Type> DocumentTypesByRelationshipName { get; }

    protected NonPrimaryEndpointMetadata(IDictionary<string, Type> documentTypesByRelationshipName)
    {
        ArgumentNullException.ThrowIfNull(documentTypesByRelationshipName);

        DocumentTypesByRelationshipName = documentTypesByRelationshipName;
    }
}
