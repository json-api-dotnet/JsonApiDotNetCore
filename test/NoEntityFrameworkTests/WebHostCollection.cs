using DotNetCoreDocs;
using DotNetCoreDocs.Writers;
using JsonApiDotNetCoreExample;
using Xunit;

namespace NoEntityFrameworkTests
{
    [CollectionDefinition("WebHostCollection")]
    public class WebHostCollection 
        : ICollectionFixture<DocsFixture<Startup, JsonDocWriter>>
    { }
}
