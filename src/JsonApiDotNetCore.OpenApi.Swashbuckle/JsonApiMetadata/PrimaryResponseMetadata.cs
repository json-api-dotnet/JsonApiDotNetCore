namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata;

internal sealed class PrimaryResponseMetadata : IJsonApiResponseMetadata
{
    public Type DocumentType { get; }

    public PrimaryResponseMetadata(Type documentType)
    {
        ArgumentNullException.ThrowIfNull(documentType);

        DocumentType = documentType;
    }
}
