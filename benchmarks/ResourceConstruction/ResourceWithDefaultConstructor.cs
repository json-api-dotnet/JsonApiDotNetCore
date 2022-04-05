using JsonApiDotNetCore.Resources;

namespace Benchmarks.ResourceConstruction;

public sealed class ResourceWithDefaultConstructor : Identifiable<long>
{
    public string? Value { get; set; }
}
