using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.SparseFieldSets;

public sealed class ResourceCaptureStore
{
    internal List<IIdentifiable> Resources { get; } = new();

    internal void Add(IEnumerable<IIdentifiable> resources)
    {
        Resources.AddRange(resources);
    }

    internal void Clear()
    {
        Resources.Clear();
    }
}
