namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata.Documents;

internal sealed class PrimaryRequestMetadata : IJsonApiRequestMetadata
{
    public Type DocumentType { get; }

    public PrimaryRequestMetadata(Type documentType)
    {
        ArgumentNullException.ThrowIfNull(documentType);

        DocumentType = documentType;
    }
}
