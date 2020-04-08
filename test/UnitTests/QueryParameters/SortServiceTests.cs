using System.Collections.Generic;
using System.Net;
using JsonApiDotNetCore.Exceptions;
using JsonApiDotNetCore.Query;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace UnitTests.QueryParameters
{
    public sealed class SortServiceTests : QueryParametersUnitTestCollection
    {
        public SortService GetService()
        {
            return new SortService(MockResourceDefinitionProvider(), _resourceGraph, MockCurrentRequest(_articleResourceContext));
        }

        [Fact]
        public void CanParse_SortService_SucceedOnMatch()
        {
            // Arrange
            var service = GetService();

            // Act
            bool result = service.CanParse("sort");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanParse_SortService_FailOnMismatch()
        {
            // Arrange
            var service = GetService();

            // Act
            bool result = service.CanParse("sorting");

            // Assert
            Assert.False(result);
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
            var exception = Assert.Throws<InvalidQueryStringParameterException>(() => sortService.Parse(query.Key, query.Value));
            
            Assert.Equal("sort", exception.QueryParameterName);
            Assert.Equal(HttpStatusCode.BadRequest, exception.Error.StatusCode);
            Assert.Equal("The list of fields to sort on contains empty elements.", exception.Error.Title);
            Assert.Null(exception.Error.Detail);
            Assert.Equal("sort", exception.Error.Source.Parameter);
        }
    }
}
