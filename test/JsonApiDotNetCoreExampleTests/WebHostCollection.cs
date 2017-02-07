using DotNetCoreDocs;
using DotNetCoreDocs.Writers;
using JsonApiDotNetCoreExample;
using Xunit;

namespace JsonApiDotNetCoreExampleTests
{
    [CollectionDefinition("WebHostCollection")]
    public class WebHostCollection 
        : ICollectionFixture<DocsFixture<Startup, JsonDocWriter>>
    { }
}
