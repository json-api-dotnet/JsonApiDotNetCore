using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace ResourceEntitySeparationExampleTests
{
    [CollectionDefinition("TestCollection")]
    public class TestCollection : ICollectionFixture<TestFixture>
    { }
}
