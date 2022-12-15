namespace JsonApiDotNetCore.OpenApi.JsonApiMetadata;

internal sealed class PrimaryRequestMetadata : IJsonApiRequestMetadata
{
    public Type DocumentType { get; }

    public PrimaryRequestMetadata(Type documentType)
    {
        ArgumentGuard.NotNull(documentType);

        DocumentType = documentType;
    }
}
