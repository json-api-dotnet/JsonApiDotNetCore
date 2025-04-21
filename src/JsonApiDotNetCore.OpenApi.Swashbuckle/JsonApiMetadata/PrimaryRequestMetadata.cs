namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata;

internal sealed class PrimaryRequestMetadata : IJsonApiRequestMetadata
{
    public Type DocumentType { get; }

    public PrimaryRequestMetadata(Type documentType)
    {
        ArgumentNullException.ThrowIfNull(documentType);

        DocumentType = documentType;
    }
}
