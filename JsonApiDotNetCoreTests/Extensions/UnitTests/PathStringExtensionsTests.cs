using Xunit;
using JsonApiDotNetCore.Extensions;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCoreTests.Extensions.UnitTests
{
    public class PathStringExtensionsTests
    {
        [Theory]
        [InlineData("/todoItems", "todoItems", "/")]
        [InlineData("/todoItems/1", "todoItems", "/1")]
        [InlineData("/1/relationships/person", "1", "/relationships/person")]
        [InlineData("/relationships/person", "relationships", "/person")]
        public void ExtractFirstSegment_Removes_And_Returns_FirstSegementInPathString(string path, string expectedFirstSegment, string expectedRemainder)
        {
            // arrange
            var pathString = new PathString(path);

            // act
            PathString remainingPath;
            var firstSegment = pathString.ExtractFirstSegment(out remainingPath);

            // assert
            Assert.Equal(expectedFirstSegment, firstSegment);
            Assert.Equal(expectedRemainder, remainingPath);
        }
    }
}
