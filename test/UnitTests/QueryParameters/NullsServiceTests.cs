using System.Net;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Exceptions;
using JsonApiDotNetCore.Query;
using Newtonsoft.Json;
using Xunit;

namespace UnitTests.QueryParameters
{
    public sealed class NullsServiceTests : QueryParametersUnitTestCollection
    {
        public NullsService GetService(NullValueHandling defaultValue, bool allowOverride)
        {
            var options = new JsonApiOptions
            {
                SerializerSettings = { NullValueHandling = defaultValue },
                AllowQueryStringOverrideForSerializerNullValueHandling = allowOverride
            };

            return new NullsService(options);
        }

        [Fact]
        public void CanParse_NullsService_SucceedOnMatch()
        {
            // Arrange
            var service = GetService(NullValueHandling.Include, true);

            // Act
            bool result = service.CanParse("nulls");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanParse_NullsService_FailOnMismatch()
        {
            // Arrange
            var service = GetService(NullValueHandling.Include, true);

            // Act
            bool result = service.CanParse("nullsettings");

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("false", NullValueHandling.Ignore, false, NullValueHandling.Ignore)]
        [InlineData("true", NullValueHandling.Ignore, false, NullValueHandling.Ignore)]
        [InlineData("false", NullValueHandling.Include, false, NullValueHandling.Include)]
        [InlineData("true", NullValueHandling.Include, false, NullValueHandling.Include)]
        [InlineData("false", NullValueHandling.Ignore, true, NullValueHandling.Ignore)]
        [InlineData("true", NullValueHandling.Ignore, true, NullValueHandling.Include)]
        [InlineData("false", NullValueHandling.Include, true, NullValueHandling.Ignore)]
        [InlineData("true", NullValueHandling.Include, true, NullValueHandling.Include)]
        public void Parse_QueryConfigWithApiSettings_Succeeds(string queryValue, NullValueHandling defaultValue, bool allowOverride, NullValueHandling expected)
        {
            // Arrange
            const string parameterName = "nulls";
            var service = GetService(defaultValue, allowOverride);

            // Act
            if (service.CanParse(parameterName) && service.IsEnabled(DisableQueryAttribute.Empty))
            {
                service.Parse(parameterName, queryValue);
            }

            // Assert
            Assert.Equal(expected, service.SerializerNullValueHandling);
        }

        [Fact]
        public void Parse_NullsService_FailOnNonBooleanValue()
        {
            // Arrange
            const string parameterName = "nulls";
            var service = GetService(NullValueHandling.Include, true);

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
