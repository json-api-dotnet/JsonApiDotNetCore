using System.Collections.Generic;
using System.Net;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Exceptions;
using JsonApiDotNetCore.Query;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Xunit;

namespace UnitTests.QueryParameters
{
    public sealed class DefaultsServiceTests : QueryParametersUnitTestCollection
    {
        public DefaultsService GetService(bool defaultValue, bool allowOverride)
        {
            var options = new JsonApiOptions
            {
                SerializerSettings =
                {
                    DefaultValueHandling = defaultValue ? DefaultValueHandling.Ignore : DefaultValueHandling.Include
                },
                AllowQueryStringOverrideForSerializerDefaultValueHandling = allowOverride
            };

            return new DefaultsService(options);
        }

        [Fact]
        public void CanParse_DefaultsService_SucceedOnMatch()
        {
            // Arrange
            var service = GetService(true, true);

            // Act
            bool result = service.CanParse("defaults");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanParse_DefaultsService_FailOnMismatch()
        {
            // Arrange
            var service = GetService(true, true);

            // Act
            bool result = service.CanParse("defaultsettings");

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("true", true, true, false)]
        [InlineData("true", true, false, true)]
        [InlineData("false", false, true, true)]
        [InlineData("false", false, false, false)]
        public void Parse_QueryConfigWithApiSettings_CanParse(string queryValue, bool defaultValue, bool allowOverride, bool expected)
        {
            // Arrange
            var query = new KeyValuePair<string, StringValues>("defaults", queryValue);
            var service = GetService(defaultValue, allowOverride);

            // Act
            if (service.CanParse(query.Key) && service.IsEnabled(DisableQueryAttribute.Empty))
            {
                service.Parse(query.Key, query.Value);
            }

            // Assert
            Assert.Equal(expected, service.OmitAttributeIfValueIsDefault);
        }

        [Fact]
        public void Parse_DefaultsService_FailOnNonBooleanValue()
        {
            // Arrange
            const string parameterName = "defaults";
            var service = GetService(true, true);

            // Act, assert
            var exception = Assert.Throws<InvalidQueryStringParameterException>(() => service.Parse(parameterName, "some"));

            Assert.Equal(parameterName, exception.QueryParameterName);
            Assert.Equal(HttpStatusCode.BadRequest, exception.Error.StatusCode);
            Assert.Equal("The specified query string value must be 'true' or 'false'.", exception.Error.Title);
            Assert.Equal($"The value 'some' for parameter '{parameterName}' is not a valid boolean value.", exception.Error.Detail);
            Assert.Equal(parameterName, exception.Error.Source.Parameter);
        }
    }
}
