using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExampleTests.Acceptance;
using Xunit;

namespace JsonApiDotNetCoreExampleTests
{
    [CollectionDefinition("WebHostCollection")]
    public class WebHostCollection
        : ICollectionFixture<TestFixture<Startup>>
    { }
}
