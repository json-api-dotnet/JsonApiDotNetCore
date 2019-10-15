using System.Collections.Generic;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Query;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace UnitTests.QueryParameters
{
    public class OmitDefaultServiceTests : QueryParametersUnitTestCollection
    {
        public OmitDefaultService GetService(bool @default, bool @override)
        {
            var options = new JsonApiOptions
            {
                DefaultAttributeResponseBehavior = new DefaultAttributeResponseBehavior(@default, @override)
            };

            return new OmitDefaultService(options);
        }

        [Fact]
        public void Name_OmitNullService_IsCorrect()
        {
            // arrange
            var service = GetService(true, true);

            // act
            var name = service.Name;

            // assert
            Assert.Equal("omitdefault", name);
        }

        [Theory]
        [InlineData("false", true, true, false)]
        [InlineData("false", true, false, true)]
        [InlineData("true", false, true, true)]
        [InlineData("true", false, false, false)]
        public void Parse_QueryConfigWithApiSettings_CanParse(string queryConfig, bool @default, bool @override, bool expected)
        {
            // arrange
            var query = new KeyValuePair<string, StringValues>($"omitNull", new StringValues(queryConfig));
            var service = GetService(@default, @override);

            // act
            service.Parse(query);

            // assert
            Assert.Equal(expected, service.Config);
        }
    }
}
