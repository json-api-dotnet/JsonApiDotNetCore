using JsonApiDotNetCore.Serialization.Response;

namespace Benchmarks.Tools;

/// <summary>
/// Doesn't produce any top-level meta.
/// </summary>
internal sealed class NoMetaBuilder : IMetaBuilder
{
    public void Add(IDictionary<string, object?> values)
    {
    }

    public IDictionary<string, object?>? Build()
    {
        return null;
    }
}
