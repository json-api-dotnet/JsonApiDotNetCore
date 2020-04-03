using System.Collections.Generic;
using System.Net;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Exceptions;
using JsonApiDotNetCore.Query;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace UnitTests.QueryParameters
{
    public sealed class PageServiceTests : QueryParametersUnitTestCollection
    {
        public PageService GetService(int? maximumPageSize = null, int? maximumPageNumber = null)
        {
            return new PageService(new JsonApiOptions
            {
                MaximumPageSize = maximumPageSize,
                MaximumPageNumber = maximumPageNumber
            });
        }

        [Fact]
        public void CanParse_FilterService_SucceedOnMatch()
        {
            // Arrange
            var filterService = GetService();

            // Act
            bool result = filterService.CanParse("page[size]");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanParse_FilterService_FailOnMismatch()
        {
            // Arrange
            var filterService = GetService();

            // Act
            bool result = filterService.CanParse("page[some]");

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("1", 1, null, false)]
        [InlineData("abcde", 0, null, true)]
        [InlineData("", 0, null, true)]
        [InlineData("5", 5, 10, false)]
        [InlineData("5", 5, 3, true)]
        public void Parse_PageSize_CanParse(string value, int expectedValue, int? maximumPageSize, bool shouldThrow)
        {
            // Arrange
            var query = new KeyValuePair<string, StringValues>("page[size]", value);
            var service = GetService(maximumPageSize: maximumPageSize);

            // Act
            if (shouldThrow)
            {
                var exception = Assert.Throws<InvalidQueryStringParameterException>(() => service.Parse(query.Key, query.Value));

                Assert.Equal("page[size]", exception.QueryParameterName);
                Assert.Equal(HttpStatusCode.BadRequest, exception.Error.StatusCode);
                Assert.Equal("The specified value is not in the range of valid values.", exception.Error.Title);
                Assert.StartsWith($"Value '{value}' is invalid, because it must be a whole number that is greater than zero", exception.Error.Detail);
                Assert.Equal("page[size]", exception.Error.Source.Parameter);
            }
            else
            {
                service.Parse(query.Key, query.Value);
                Assert.Equal(expectedValue, service.PageSize);
            }
        }

        [Theory]
        [InlineData("1", 1, null, false)]
        [InlineData("abcde", 0, null, true)]
        [InlineData("", 0, null, true)]
        [InlineData("5", 5, 10, false)]
        [InlineData("5", 5, 3, true)]
        public void Parse_PageNumber_CanParse(string value, int expectedValue, int? maximumPageNumber, bool shouldThrow)
        {
            // Arrange
            var query = new KeyValuePair<string, StringValues>("page[number]", value);
            var service = GetService(maximumPageNumber: maximumPageNumber);

            // Act
            if (shouldThrow)
            {
                var exception = Assert.Throws<InvalidQueryStringParameterException>(() => service.Parse(query.Key, query.Value));

                Assert.Equal("page[number]", exception.QueryParameterName);
                Assert.Equal(HttpStatusCode.BadRequest, exception.Error.StatusCode);
                Assert.Equal("The specified value is not in the range of valid values.", exception.Error.Title);
                Assert.StartsWith($"Value '{value}' is invalid, because it must be a whole number that is non-zero", exception.Error.Detail);
                Assert.Equal("page[number]", exception.Error.Source.Parameter);
            }
            else
            {
                service.Parse(query.Key, query.Value);
                Assert.Equal(expectedValue, service.CurrentPage);
            }
        }
    }
}
