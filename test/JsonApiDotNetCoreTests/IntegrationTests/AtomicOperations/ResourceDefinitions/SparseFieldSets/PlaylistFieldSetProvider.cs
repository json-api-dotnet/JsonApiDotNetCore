namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.ResourceDefinitions.SparseFieldSets;

public sealed class PlaylistFieldSetProvider
{
    internal bool IsEnabled { get; set; }
    internal int QueryCallCount { get; set; }
}
