#nullable disable

using Xunit;

namespace TestBuildingBlocks
{
    public sealed class DummyTest
    {
        [Fact]
        public void Empty()
        {
            // This dummy test exists solely to suppress the warning
            // during test runs that no tests were found in this project.
        }
    }
}
