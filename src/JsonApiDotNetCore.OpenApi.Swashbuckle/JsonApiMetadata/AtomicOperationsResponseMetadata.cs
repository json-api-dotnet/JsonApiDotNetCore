namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata;

internal sealed class AtomicOperationsResponseMetadata : IJsonApiResponseMetadata
{
    public static AtomicOperationsResponseMetadata Instance { get; } = new();

    private AtomicOperationsResponseMetadata()
    {
    }
}
