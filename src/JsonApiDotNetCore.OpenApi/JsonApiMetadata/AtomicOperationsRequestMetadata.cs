namespace JsonApiDotNetCore.OpenApi.JsonApiMetadata;

internal sealed class AtomicOperationsRequestMetadata : IJsonApiRequestMetadata
{
    public static AtomicOperationsRequestMetadata Instance { get; } = new();

    private AtomicOperationsRequestMetadata()
    {
    }
}
