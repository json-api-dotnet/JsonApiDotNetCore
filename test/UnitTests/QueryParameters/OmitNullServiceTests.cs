using System.Collections.Generic;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Query;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace UnitTests.QueryParameters
{
    public sealed class OmitNullServiceTests : QueryParametersUnitTestCollection
    {
        public OmitNullService GetService(bool @default, bool @override)
        {
            var options = new JsonApiOptions
            {
                NullAttributeResponseBehavior = new NullAttributeResponseBehavior(@default, @override)
            };

            return new OmitNullService(options);
        }

        [Fact]
        public void CanParse_FilterService_SucceedOnMatch()
        {
            // Arrange
            var filterService = GetService(true, true);

            // Act
            bool result = filterService.CanParse("omitNull");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanParse_FilterService_FailOnMismatch()
        {
            // Arrange
            var filterService = GetService(true, true);

            // Act
            bool result = filterService.CanParse("omit-null");

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("false", true, true, false)]
        [InlineData("false", true, false, true)]
        [InlineData("true", false, true, true)]
        [InlineData("true", false, false, false)]
        public void Parse_QueryConfigWithApiSettings_CanParse(string queryConfig, bool @default, bool @override, bool expected)
        {
            // Arrange
            var query = new KeyValuePair<string, StringValues>("omitNull", new StringValues(queryConfig));
            var service = GetService(@default, @override);

            // Act
            if (service.CanParse(query.Key) && service.IsEnabled(DisableQueryAttribute.Empty))
            {
                service.Parse(query.Key, query.Value);
            }

            // Assert
            Assert.Equal(expected, service.OmitAttributeIfValueIsNull);
        }
    }
}
