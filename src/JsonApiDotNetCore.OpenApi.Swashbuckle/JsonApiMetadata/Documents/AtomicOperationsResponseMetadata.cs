using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Documents;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata.Documents;

internal sealed class AtomicOperationsResponseMetadata : IJsonApiResponseMetadata
{
    public static AtomicOperationsResponseMetadata Instance { get; } = new();

    public Type DocumentType => typeof(OperationsResponseDocument);

    private AtomicOperationsResponseMetadata()
    {
    }
}
