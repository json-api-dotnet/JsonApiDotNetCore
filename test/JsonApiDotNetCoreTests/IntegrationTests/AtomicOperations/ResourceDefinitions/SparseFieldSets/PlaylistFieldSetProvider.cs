namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.ResourceDefinitions.SparseFieldSets;

public sealed class PlaylistFieldSetProvider
{
    public bool IsEnabled { get; set; }
    public int QueryCallCount { get; set; }
}
