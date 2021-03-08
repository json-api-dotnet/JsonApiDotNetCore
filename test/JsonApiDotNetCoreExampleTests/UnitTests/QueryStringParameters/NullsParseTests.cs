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
    public sealed class NullsParseTests
    {
        private readonly INullsQueryStringParameterReader _reader;

        public NullsParseTests()
        {
            _reader = new NullsQueryStringParameterReader(new JsonApiOptions());
        }

        [Theory]
        [InlineData("nulls", true)]
        [InlineData("null", false)]
        [InlineData("nullsettings", false)]
        public void Reader_Supports_Parameter_Name(string parameterName, bool expectCanParse)
        {
            // Act
            bool canParse = _reader.CanRead(parameterName);

            // Assert
            canParse.Should().Be(expectCanParse);
        }

        [Theory]
        [InlineData(StandardQueryStringParameters.Nulls, false, false)]
        [InlineData(StandardQueryStringParameters.Nulls, true, false)]
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
                AllowQueryStringOverrideForSerializerNullValueHandling = allowOverride
            };

            var reader = new NullsQueryStringParameterReader(options);

            // Act
            bool isEnabled = reader.IsEnabled(new DisableQueryStringAttribute(parametersDisabled));

            // Assert
            isEnabled.Should().Be(allowOverride && expectIsEnabled);
        }

        [Theory]
        [InlineData("nulls", "", "The value '' must be 'true' or 'false'.")]
        [InlineData("nulls", " ", "The value ' ' must be 'true' or 'false'.")]
        [InlineData("nulls", "null", "The value 'null' must be 'true' or 'false'.")]
        [InlineData("nulls", "0", "The value '0' must be 'true' or 'false'.")]
        [InlineData("nulls", "1", "The value '1' must be 'true' or 'false'.")]
        [InlineData("nulls", "-1", "The value '-1' must be 'true' or 'false'.")]
        public void Reader_Read_Fails(string parameterName, string parameterValue, string errorMessage)
        {
            // Act
            Action action = () => _reader.Read(parameterName, parameterValue);

            // Assert
            InvalidQueryStringParameterException exception = action.Should().ThrowExactly<InvalidQueryStringParameterException>().And;

            exception.QueryParameterName.Should().Be(parameterName);
            exception.Errors.Should().HaveCount(1);
            exception.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            exception.Errors[0].Title.Should().Be("The specified nulls is invalid.");
            exception.Errors[0].Detail.Should().Be(errorMessage);
            exception.Errors[0].Source.Parameter.Should().Be(parameterName);
        }

        [Theory]
        [InlineData("nulls", "true", NullValueHandling.Include)]
        [InlineData("nulls", "True", NullValueHandling.Include)]
        [InlineData("nulls", "false", NullValueHandling.Ignore)]
        [InlineData("nulls", "False", NullValueHandling.Ignore)]
        public void Reader_Read_Succeeds(string parameterName, string parameterValue, NullValueHandling expectedValue)
        {
            // Act
            _reader.Read(parameterName, parameterValue);

            NullValueHandling handling = _reader.SerializerNullValueHandling;

            // Assert
            handling.Should().Be(expectedValue);
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
        public void Reader_Outcome(string queryStringParameterValue, NullValueHandling optionsNullValue, bool optionsAllowOverride, NullValueHandling expected)
        {
            // Arrange
            var options = new JsonApiOptions
            {
                SerializerSettings =
                {
                    NullValueHandling = optionsNullValue
                },
                AllowQueryStringOverrideForSerializerNullValueHandling = optionsAllowOverride
            };

            var reader = new NullsQueryStringParameterReader(options);

            // Act
            if (reader.IsEnabled(DisableQueryStringAttribute.Empty))
            {
                reader.Read("nulls", queryStringParameterValue);
            }

            // Assert
            reader.SerializerNullValueHandling.Should().Be(expected);
        }
    }
}
