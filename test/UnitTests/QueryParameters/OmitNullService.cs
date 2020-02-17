using System.Collections.Generic;
using JsonApiDotNetCore.Configuration;
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
        public void Name_OmitNullService_IsCorrect()
        {
            // Arrange
            var service = GetService(true, true);

            // Act
            var name = service.Name;

            // Assert
            Assert.Equal("omitnull", name);
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
            service.Parse(query);

            // Assert
            Assert.Equal(expected, service.Config);
        }
    }
}
