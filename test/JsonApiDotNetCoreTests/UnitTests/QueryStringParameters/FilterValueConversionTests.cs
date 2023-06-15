using System.ComponentModel.Design;
using System.Net;
using FluentAssertions;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Queries.Internal.Parsing;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.QueryStrings.Internal;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.QueryStringParameters;

public sealed class FilterValueConversionTests : BaseParseTests
{
    [Fact]
    public void Throws_when_converter_returns_null()
    {
        // Arrange
        var converter = new NullConverter();

        var resourceFactory = new ResourceFactory(new ServiceContainer());
        var reader = new FilterQueryStringParameterReader(Request, ResourceGraph, resourceFactory, Options, converter.AsEnumerable());

        // Act
        Action action = () => reader.Read("filter", "equals(title,'some')");

        // Assert
        action.Should().ThrowExactly<InvalidOperationException>().WithMessage(
            "Converter 'NullConverter' returned null for 'some' on attribute 'title'. Return a sentinel value instead.");
    }

    [Fact]
    public void Wraps_error_when_QueryParseException_is_thrown()
    {
        // Arrange
        var converter = new ThrowingConverter();

        var resourceFactory = new ResourceFactory(new ServiceContainer());
        var reader = new FilterQueryStringParameterReader(Request, ResourceGraph, resourceFactory, Options, converter.AsEnumerable());

        // Act
        Action action = () => reader.Read("filter", "equals(title,'some')");

        // Assert
        JsonApiException exception = action.Should().ThrowExactly<InvalidQueryStringParameterException>().Which;
        exception.Errors.ShouldHaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified filter is invalid.");
        error.Detail.Should().Be("Unable to parse 'some'.");
        error.Source.ShouldNotBeNull();
        error.Source.Parameter.Should().Be("filter");
    }

    private sealed class NullConverter : IFilterValueConverter
    {
        public bool CanConvert(AttrAttribute attribute)
        {
            return true;
        }

        public object Convert(AttrAttribute attribute, string value, Type outerExpressionType)
        {
            return null!;
        }
    }

    private sealed class ThrowingConverter : IFilterValueConverter
    {
        public bool CanConvert(AttrAttribute attribute)
        {
            return true;
        }

        public object Convert(AttrAttribute attribute, string value, Type outerExpressionType)
        {
            throw new QueryParseException($"Unable to parse '{value}'.");
        }
    }
}
