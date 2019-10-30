using System.Collections.Generic;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Query;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace UnitTests.QueryParameters
{
    public class PageServiceTests : QueryParametersUnitTestCollection
    {
        public PageService GetService()
        {
            return new PageService(new JsonApiOptions());
        }

        [Fact]
        public void Name_PageService_IsCorrect()
        {
            // Arrange
            var filterService = GetService();

            // Act
            var name = filterService.Name;

            // Assert
            Assert.Equal("page", name);
        }

        [Theory]
        [InlineData("1", 1, false)]
        [InlineData("abcde", 0, true)]
        [InlineData("", 0, true)]
        public void Parse_PageSize_CanParse(string value, int expectedValue, bool shouldThrow)
        {
            // Arrange
            var query = new KeyValuePair<string, StringValues>($"page[size]", new StringValues(value));
            var service = GetService();

            // Act
            if (shouldThrow)
            {
                var ex = Assert.Throws<JsonApiException>(() => service.Parse(query));
                Assert.Equal(400, ex.GetStatusCode());
            }
            else
            {
                service.Parse(query);
                Assert.Equal(expectedValue, service.PageSize);
            }
        }

        [Theory]
        [InlineData("1", 1, false)]
        [InlineData("abcde", 0, true)]
        [InlineData("", 0, true)]
        public void Parse_PageNumber_CanParse(string value, int expectedValue, bool shouldThrow)
        {
            // Arrange
            var query = new KeyValuePair<string, StringValues>($"page[number]", new StringValues(value));
            var service = GetService();


            // Act
            if (shouldThrow)
            {
                var ex = Assert.Throws<JsonApiException>(() => service.Parse(query));
                Assert.Equal(400, ex.GetStatusCode());
            }
            else
            {
                service.Parse(query);
                Assert.Equal(expectedValue, service.CurrentPage);
            }
        }
    }
}
