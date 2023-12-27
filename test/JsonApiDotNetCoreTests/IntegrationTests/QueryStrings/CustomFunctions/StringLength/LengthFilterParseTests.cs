using System.ComponentModel.Design;
using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreTests.UnitTests.QueryStringParameters;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.StringLength;

public sealed class LengthFilterParseTests : BaseParseTests
{
    private readonly FilterQueryStringParameterReader _reader;

    public LengthFilterParseTests()
    {
        var resourceFactory = new ResourceFactory(new ServiceContainer());
        var scopeParser = new QueryStringParameterScopeParser();
        var valueParser = new LengthFilterParser(resourceFactory);

        _reader = new FilterQueryStringParameterReader(scopeParser, valueParser, Request, ResourceGraph, Options);
    }

    [Theory]
    [InlineData("filter", "equals(length^", "( expected.")]
    [InlineData("filter", "equals(length(^", "Field name expected.")]
    [InlineData("filter", "equals(length(^ ", "Unexpected whitespace.")]
    [InlineData("filter", "equals(length(^)", "Field name expected.")]
    [InlineData("filter", "equals(length(^'a')", "Field name expected.")]
    [InlineData("filter", "equals(length(^some)", "Field 'some' does not exist on resource type 'blogs'.")]
    [InlineData("filter", "equals(length(^caption)", "Field 'caption' does not exist on resource type 'blogs'.")]
    [InlineData("filter", "equals(length(^null)", "Field name expected.")]
    [InlineData("filter", "equals(length(title)^)", ", expected.")]
    [InlineData("filter", "equals(length(owner.preferences.^useDarkTheme)", "Attribute of type 'String' expected.")]
    public void Reader_Read_Fails(string parameterName, string parameterValue, string errorMessage)
    {
        // Arrange
        var parameterValueSource = new MarkedText(parameterValue, '^');

        // Act
        Action action = () => _reader.Read(parameterName, parameterValueSource.Text);

        // Assert
        InvalidQueryStringParameterException exception = action.Should().ThrowExactly<InvalidQueryStringParameterException>().And;

        exception.ParameterName.Should().Be(parameterName);
        exception.Errors.ShouldHaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified filter is invalid.");
        error.Detail.Should().Be($"{errorMessage} {parameterValueSource}");
        error.Source.ShouldNotBeNull();
        error.Source.Parameter.Should().Be(parameterName);
    }

    [Theory]
    [InlineData("filter", "equals(length(title),'1')", null)]
    [InlineData("filter", "greaterThan(length(owner.userName),'1')", null)]
    [InlineData("filter", "has(posts,lessThan(length(author.userName),'1'))", null)]
    [InlineData("filter", "or(equals(length(title),'1'),equals(length(platformName),'1'))", null)]
    [InlineData("filter[posts]", "equals(length(author.userName),'1')", "posts")]
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
