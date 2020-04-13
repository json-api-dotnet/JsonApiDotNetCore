using System.Net;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Exceptions;
using JsonApiDotNetCore.Query;
using Newtonsoft.Json;
using Xunit;

namespace UnitTests.QueryParameters
{
    public sealed class DefaultsServiceTests : QueryParametersUnitTestCollection
    {
        public DefaultsService GetService(DefaultValueHandling defaultValue, bool allowOverride)
        {
            var options = new JsonApiOptions
            {
                SerializerSettings = { DefaultValueHandling = defaultValue },
                AllowQueryStringOverrideForSerializerDefaultValueHandling = allowOverride
            };

            return new DefaultsService(options);
        }

        [Fact]
        public void CanParse_DefaultsService_SucceedOnMatch()
        {
            // Arrange
            var service = GetService(DefaultValueHandling.Include, true);

            // Act
            bool result = service.CanParse("defaults");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanParse_DefaultsService_FailOnMismatch()
        {
            // Arrange
            var service = GetService(DefaultValueHandling.Include, true);

            // Act
            bool result = service.CanParse("defaultsettings");

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("false", DefaultValueHandling.Ignore, false, DefaultValueHandling.Ignore)]
        [InlineData("true", DefaultValueHandling.Ignore, false, DefaultValueHandling.Ignore)]
        [InlineData("false", DefaultValueHandling.Include, false, DefaultValueHandling.Include)]
        [InlineData("true", DefaultValueHandling.Include, false, DefaultValueHandling.Include)]
        [InlineData("false", DefaultValueHandling.Ignore, true, DefaultValueHandling.Ignore)]
        [InlineData("true", DefaultValueHandling.Ignore, true, DefaultValueHandling.Include)]
        [InlineData("false", DefaultValueHandling.Include, true, DefaultValueHandling.Ignore)]
        [InlineData("true", DefaultValueHandling.Include, true, DefaultValueHandling.Include)]
        public void Parse_QueryConfigWithApiSettings_Succeeds(string queryValue, DefaultValueHandling defaultValue, bool allowOverride, DefaultValueHandling expected)
        {
            // Arrange
            const string parameterName = "defaults";
            var service = GetService(defaultValue, allowOverride);

            // Act
            if (service.CanParse(parameterName) && service.IsEnabled(DisableQueryAttribute.Empty))
            {
                service.Parse(parameterName, queryValue);
            }

            // Assert
            Assert.Equal(expected, service.SerializerDefaultValueHandling);
        }

        [Fact]
        public void Parse_DefaultsService_FailOnNonBooleanValue()
        {
            // Arrange
            const string parameterName = "defaults";
            var service = GetService(DefaultValueHandling.Include, true);

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
