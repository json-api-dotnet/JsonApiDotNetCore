namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata.Documents;

internal sealed class PrimaryResponseMetadata(Type? documentType) : IJsonApiResponseMetadata
{
    public Type? DocumentType { get; } = documentType;
}
