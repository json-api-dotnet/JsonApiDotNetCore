using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Query;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace UnitTests.QueryParameters
{
    public sealed class FilterServiceTests : QueryParametersUnitTestCollection
    {
        public FilterService GetService()
        {
            return new FilterService(MockResourceDefinitionProvider(), _resourceGraph, MockCurrentRequest(_articleResourceContext));
        }

        [Fact]
        public void CanParse_FilterService_SucceedOnMatch()
        {
            // Arrange
            var filterService = GetService();

            // Act
            bool result = filterService.CanParse("filter[age]");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanParse_FilterService_FailOnMismatch()
        {
            // Arrange
            var filterService = GetService();

            // Act
            bool result = filterService.CanParse("other");

            // Assert
            Assert.False(result);
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
            var query = new KeyValuePair<string, StringValues>($"filter[{key}]", queryValue);
            var filterService = GetService();

            // Act
            filterService.Parse(query.Key, query.Value);
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
