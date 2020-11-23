using OperationsExample;
using Xunit;

namespace OperationsExampleTests
{
    [CollectionDefinition("WebHostCollection")]
    public class WebHostCollection : ICollectionFixture<TestFixture<TestStartup>>
    {
    }
}
