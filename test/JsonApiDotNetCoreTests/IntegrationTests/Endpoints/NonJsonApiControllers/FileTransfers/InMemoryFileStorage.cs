using System.Collections.Concurrent;

namespace JsonApiDotNetCoreTests.IntegrationTests.Endpoints.NonJsonApiControllers.FileTransfers;

public sealed class InMemoryFileStorage
{
    internal ConcurrentDictionary<string, byte[]> Files { get; } = new();
}
