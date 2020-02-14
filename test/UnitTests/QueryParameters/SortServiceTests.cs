using System.Collections.Generic;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Query;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace UnitTests.QueryParameters
{
    public class SortServiceTests : QueryParametersUnitTestCollection
    {
        public SortService GetService()
        {
            return new SortService(MockResourceDefinitionProvider(), _resourceGraph, MockCurrentRequest(_articleResourceContext));
        }

        [Fact]
        public void Name_SortService_IsCorrect()
        {
            // Arrange
            var filterService = GetService();

            // Act
            var name = filterService.Name;

            // Assert
            Assert.Equal("sort", name);
        }

        [Theory]
        [InlineData("text,,1")]
        [InlineData("text,hello,,5")]
        [InlineData(",,2")]
        public void Parse_InvalidSortQuery_ThrowsJsonApiException(string stringSortQuery)
        {
            // Arrange
            var query = new KeyValuePair<string, StringValues>("sort", stringSortQuery);
            var sortService = GetService();

            // Act, assert
            var exception = Assert.Throws<JsonApiException>(() => sortService.Parse(query));
            Assert.Contains("sort", exception.Message);
        }
    }
}
