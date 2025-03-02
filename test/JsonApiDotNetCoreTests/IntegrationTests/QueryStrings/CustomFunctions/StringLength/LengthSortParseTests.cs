using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreTests.UnitTests.QueryStringParameters;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.StringLength;

public sealed class LengthSortParseTests : BaseParseTests
{
    private readonly SortQueryStringParameterReader _reader;

    public LengthSortParseTests()
    {
        var scopeParser = new QueryStringParameterScopeParser();
        var valueParser = new LengthSortParser();

        _reader = new SortQueryStringParameterReader(scopeParser, valueParser, Request, ResourceGraph);
    }

    [Theory]
    [InlineData("sort", "length^", "( expected.")]
    [InlineData("sort", "length(^", "Field name expected.")]
    [InlineData("sort", "length(^ ", "Unexpected whitespace.")]
    [InlineData("sort", "length(^)", "Field name expected.")]
    [InlineData("sort", "length(^'a')", "Field name expected.")]
    [InlineData("sort", "length(^some)", "Field 'some' does not exist on resource type 'blogs'.")]
    [InlineData("sort", "length(^caption)", "Field 'caption' does not exist on resource type 'blogs'.")]
    [InlineData("sort", "length(^null)", "Field name expected.")]
    [InlineData("sort", "length(title)^)", ", expected.")]
    [InlineData("sort", "length(owner.preferences.^useDarkTheme)", "Attribute of type 'String' expected.")]
    public void Reader_Read_Fails(string parameterName, string parameterValue, string errorMessage)
    {
        // Arrange
        var parameterValueSource = new MarkedText(parameterValue, '^');

        // Act
        Action action = () => _reader.Read(parameterName, parameterValueSource.Text);

        // Assert
        InvalidQueryStringParameterException exception = action.Should().ThrowExactly<InvalidQueryStringParameterException>().And;

        exception.ParameterName.Should().Be(parameterName);
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified sort is invalid.");
        error.Detail.Should().Be($"{errorMessage} {parameterValueSource}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be(parameterName);
    }

    [Theory]
    [InlineData("sort", "length(title)", null)]
    [InlineData("sort", "length(title),-length(platformName)", null)]
    [InlineData("sort", "length(owner.userName)", null)]
    [InlineData("sort[posts]", "length(author.userName)", "posts")]
    public void Reader_Read_Succeeds(string parameterName, string parameterValue, string? scopeExpected)
    {
        // Act
        _reader.Read(parameterName, parameterValue);

        IReadOnlyCollection<ExpressionInScope> constraints = _reader.GetConstraints();

        // Assert
        ResourceFieldChainExpression? scope = constraints.Select(expressionInScope => expressionInScope.Scope).Single();
        scope?.ToString().Should().Be(scopeExpected);

        QueryExpression value = constraints.Select(expressionInScope => expressionInScope.Expression).Single();
        value.ToString().Should().Be(parameterValue);
    }
}
