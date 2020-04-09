using System.Collections.Generic;
using System.Net;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Exceptions;
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
                SerializerOmitAttributeIfValueIsNull = @default,
                AllowOmitNullQueryStringOverride = @override
            };

            return new OmitNullService(options);
        }

        [Fact]
        public void CanParse_OmitNullService_SucceedOnMatch()
        {
            // Arrange
            var service = GetService(true, true);

            // Act
            bool result = service.CanParse("omitNull");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanParse_OmitNullService_FailOnMismatch()
        {
            // Arrange
            var service = GetService(true, true);

            // Act
            bool result = service.CanParse("omit-null");

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("false", true, true, false)]
        [InlineData("false", true, false, true)]
        [InlineData("true", false, true, true)]
        [InlineData("true", false, false, false)]
        public void Parse_QueryConfigWithApiSettings_CanParse(string queryValue, bool @default, bool @override, bool expected)
        {
            // Arrange
            var query = new KeyValuePair<string, StringValues>("omitNull", queryValue);
            var service = GetService(@default, @override);

            // Act
            if (service.CanParse(query.Key) && service.IsEnabled(DisableQueryAttribute.Empty))
            {
                service.Parse(query.Key, query.Value);
            }

            // Assert
            Assert.Equal(expected, service.OmitAttributeIfValueIsNull);
        }

        [Fact]
        public void Parse_OmitNullService_FailOnNonBooleanValue()
        {
            // Arrange
            const string parameterName = "omit-null";
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
