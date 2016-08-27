using Xunit;
using JsonApiDotNetCore.Extensions;

namespace JsonApiDotNetCoreTests.Extensions
{
    // see example explanation on xUnit.net website:
    // https://xunit.github.io/docs/getting-started-dotnet-core.html
    public class AddJsonApiAdds
    {
        [Fact]
        public void PassingTest()
        {
            Assert.Equal(4, Add(2, 2));
        }

        int Add(int x, int y)
        {
            return x + y;
        }
    }
}
