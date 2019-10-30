using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Query;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace UnitTests.QueryParameters
{
    public class FilterServiceTests : QueryParametersUnitTestCollection
    {
        public FilterService GetService()
        {
            return new FilterService(MockResourceDefinitionProvider(), _resourceGraph, MockCurrentRequest(_articleResourceContext));
        }

        [Fact]
        public void Name_FilterService_IsCorrect()
        {
            // Arrange
            var filterService = GetService();

            // Act
            var name = filterService.Name;

            // Assert
            Assert.Equal("filter", name);
        }

        [Theory]
        [InlineData("title", "", "value")]
        [InlineData("title", "eq:", "value")]
        [InlineData("title", "lt:", "value")]
        [InlineData("title", "gt:", "value")]
        [InlineData("title", "le:", "value")]
        [InlineData("title", "ge:", "value")]
        [InlineData("title", "like:", "value")]
        [InlineData("title", "ne:", "value")]
        [InlineData("title", "in:", "value")]
        [InlineData("title", "nin:", "value")]
        [InlineData("title", "isnull:", "")]
        [InlineData("title", "isnotnull:", "")]
        [InlineData("title", "", "2017-08-15T22:43:47.0156350-05:00")]
        [InlineData("title", "le:", "2017-08-15T22:43:47.0156350-05:00")]
        public void Parse_ValidFilters_CanParse(string key, string @operator, string value)
        {
            // Arrange
            var queryValue = @operator + value;
            var query = new KeyValuePair<string, StringValues>($"filter[{key}]", new StringValues(queryValue));
            var filterService = GetService();

            // Act
            filterService.Parse(query);
            var filter = filterService.Get().Single();

            // Assert
            if (!string.IsNullOrEmpty(@operator))
                Assert.Equal(@operator.Replace(":", ""), filter.Operation.ToString("G"));
            else
                Assert.Equal(FilterOperation.eq, filter.Operation);

            if (!string.IsNullOrEmpty(value))
                Assert.Equal(value, filter.Value);
        }
    }
}
