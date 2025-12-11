using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Documents;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata.Documents;

internal sealed class AtomicOperationsRequestMetadata : IJsonApiRequestMetadata
{
    public static AtomicOperationsRequestMetadata Instance { get; } = new();

    public Type DocumentType => typeof(OperationsRequestDocument);

    private AtomicOperationsRequestMetadata()
    {
    }
}
