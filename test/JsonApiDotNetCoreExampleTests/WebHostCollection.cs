using JsonApiDotNetCoreExample;
using Xunit;

namespace JsonApiDotNetCoreExampleTests
{
    [CollectionDefinition("WebHostCollection")]
    public class WebHostCollection
        : ICollectionFixture<TestFixture<Startup>>
    { }
}
