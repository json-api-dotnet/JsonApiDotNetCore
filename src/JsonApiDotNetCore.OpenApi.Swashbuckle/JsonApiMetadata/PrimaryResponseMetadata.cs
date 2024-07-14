namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata;

internal sealed class PrimaryResponseMetadata : IJsonApiResponseMetadata
{
    public Type DocumentType { get; }

    public PrimaryResponseMetadata(Type documentType)
    {
        ArgumentGuard.NotNull(documentType);

        DocumentType = documentType;
    }
}
