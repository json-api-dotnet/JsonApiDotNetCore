using JsonApiDotNetCore.Serialization.Response;

namespace JsonApiDotNetCoreTests.UnitTests.Serialization.Response;

internal sealed class FakeMetaBuilder : IMetaBuilder
{
    public void Add(IReadOnlyDictionary<string, object?> values)
    {
    }

    public IDictionary<string, object?>? Build()
    {
        return null;
    }
}
