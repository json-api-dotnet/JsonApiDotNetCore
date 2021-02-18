using System;
using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.QueryStrings.Internal;
using Newtonsoft.Json;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.UnitTests.QueryStringParameters
{
    public sealed class DefaultsParseTests
    {
        private readonly IDefaultsQueryStringParameterReader _reader;

        public DefaultsParseTests()
        {
            _reader = new DefaultsQueryStringParameterReader(new JsonApiOptions());
        }

        [Theory]
        [InlineData("defaults", true)]
        [InlineData("default", false)]
        [InlineData("defaultsettings", false)]
        public void Reader_Supports_Parameter_Name(string parameterName, bool expectCanParse)
        {
            // Act
            bool canParse = _reader.CanRead(parameterName);

            // Assert
            canParse.Should().Be(expectCanParse);
        }

        [Theory]
        [InlineData(StandardQueryStringParameters.Defaults, false, false)]
        [InlineData(StandardQueryStringParameters.Defaults, true, false)]
        [InlineData(StandardQueryStringParameters.All, false, false)]
        [InlineData(StandardQueryStringParameters.All, true, false)]
        [InlineData(StandardQueryStringParameters.None, false, false)]
        [InlineData(StandardQueryStringParameters.None, true, true)]
        [InlineData(StandardQueryStringParameters.Filter, false, false)]
        [InlineData(StandardQueryStringParameters.Filter, true, true)]
        public void Reader_Is_Enabled(StandardQueryStringParameters parametersDisabled, bool allowOverride, bool expectIsEnabled)
        {
            // Arrange
            var options = new JsonApiOptions
            {
                AllowQueryStringOverrideForSerializerDefaultValueHandling = allowOverride
            };

            var reader = new DefaultsQueryStringParameterReader(options);

            // Act
            bool isEnabled = reader.IsEnabled(new DisableQueryStringAttribute(parametersDisabled));

            // Assert
            isEnabled.Should().Be(allowOverride && expectIsEnabled);
        }

        [Theory]
        [InlineData("defaults", "", "The value '' must be 'true' or 'false'.")]
        [InlineData("defaults", " ", "The value ' ' must be 'true' or 'false'.")]
        [InlineData("defaults", "null", "The value 'null' must be 'true' or 'false'.")]
        [InlineData("defaults", "0", "The value '0' must be 'true' or 'false'.")]
        [InlineData("defaults", "1", "The value '1' must be 'true' or 'false'.")]
        [InlineData("defaults", "-1", "The value '-1' must be 'true' or 'false'.")]
        public void Reader_Read_Fails(string parameterName, string parameterValue, string errorMessage)
        {
            // Act
            Action action = () => _reader.Read(parameterName, parameterValue);

            // Assert
            InvalidQueryStringParameterException exception = action.Should().ThrowExactly<InvalidQueryStringParameterException>().And;

            exception.QueryParameterName.Should().Be(parameterName);
            exception.Errors.Should().HaveCount(1);
            exception.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            exception.Errors[0].Title.Should().Be("The specified defaults is invalid.");
            exception.Errors[0].Detail.Should().Be(errorMessage);
            exception.Errors[0].Source.Parameter.Should().Be(parameterName);
        }

        [Theory]
        [InlineData("defaults", "true", DefaultValueHandling.Include)]
        [InlineData("defaults", "True", DefaultValueHandling.Include)]
        [InlineData("defaults", "false", DefaultValueHandling.Ignore)]
        [InlineData("defaults", "False", DefaultValueHandling.Ignore)]
        public void Reader_Read_Succeeds(string parameterName, string parameterValue, DefaultValueHandling expectedValue)
        {
            // Act
            _reader.Read(parameterName, parameterValue);

            DefaultValueHandling handling = _reader.SerializerDefaultValueHandling;

            // Assert
            handling.Should().Be(expectedValue);
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
        public void Reader_Outcome(string queryStringParameterValue, DefaultValueHandling optionsDefaultValue, bool optionsAllowOverride,
            DefaultValueHandling expected)
        {
            // Arrange
            var options = new JsonApiOptions
            {
                SerializerSettings =
                {
                    DefaultValueHandling = optionsDefaultValue
                },
                AllowQueryStringOverrideForSerializerDefaultValueHandling = optionsAllowOverride
            };

            var reader = new DefaultsQueryStringParameterReader(options);

            // Act
            if (reader.IsEnabled(DisableQueryStringAttribute.Empty))
            {
                reader.Read("defaults", queryStringParameterValue);
            }

            // Assert
            reader.SerializerDefaultValueHandling.Should().Be(expected);
        }
    }
}
