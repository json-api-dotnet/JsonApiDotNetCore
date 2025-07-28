using System.Collections.Concurrent;

namespace OpenApiTests.MixedControllers;

public sealed class InMemoryFileStorage
{
    public ConcurrentDictionary<string, byte[]> Files { get; } = new();
}
