namespace JsonApiDotNetCore.OpenApi.JsonApiMetadata;

internal sealed class AtomicOperationsResponseMetadata : IJsonApiResponseMetadata
{
    public static AtomicOperationsResponseMetadata Instance { get; } = new();

    private AtomicOperationsResponseMetadata()
    {
    }
}
