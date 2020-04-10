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
    public sealed class NullsServiceTests : QueryParametersUnitTestCollection
    {
        public NullsService GetService(bool defaultValue, bool allowOverride)
        {
            var options = new JsonApiOptions
            {
                SerializerSettings =
                {
                    NullValueHandling = defaultValue ? NullValueHandling.Ignore : NullValueHandling.Include
                },
                AllowQueryStringOverrideForSerializerNullValueHandling = allowOverride
            };

            return new NullsService(options);
        }

        [Fact]
        public void CanParse_NullsService_SucceedOnMatch()
        {
            // Arrange
            var service = GetService(true, true);

            // Act
            bool result = service.CanParse("nulls");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanParse_NullsService_FailOnMismatch()
        {
            // Arrange
            var service = GetService(true, true);

            // Act
            bool result = service.CanParse("nullsettings");

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
            var query = new KeyValuePair<string, StringValues>("nulls", queryValue);
            var service = GetService(defaultValue, allowOverride);

            // Act
            if (service.CanParse(query.Key) && service.IsEnabled(DisableQueryAttribute.Empty))
            {
                service.Parse(query.Key, query.Value);
            }

            // Assert
            Assert.Equal(expected, service.OmitAttributeIfValueIsNull);
        }

        [Fact]
        public void Parse_NullsService_FailOnNonBooleanValue()
        {
            // Arrange
            const string parameterName = "nulls";
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
