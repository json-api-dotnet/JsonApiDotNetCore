namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata;

internal sealed class AtomicOperationsRequestMetadata : IJsonApiRequestMetadata
{
    public static AtomicOperationsRequestMetadata Instance { get; } = new();

    private AtomicOperationsRequestMetadata()
    {
    }
}
