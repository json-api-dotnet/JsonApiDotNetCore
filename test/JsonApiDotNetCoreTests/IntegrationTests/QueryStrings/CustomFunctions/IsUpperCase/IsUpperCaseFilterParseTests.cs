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

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.IsUpperCase;

public sealed class IsUpperCaseFilterParseTests : BaseParseTests
{
    private readonly FilterQueryStringParameterReader _reader;

    public IsUpperCaseFilterParseTests()
    {
        using var serviceProvider = new ServiceContainer();
        var resourceFactory = new ResourceFactory(serviceProvider);
        var scopeParser = new QueryStringParameterScopeParser();
        var valueParser = new IsUpperCaseFilterParser(resourceFactory);

        _reader = new FilterQueryStringParameterReader(scopeParser, valueParser, Request, ResourceGraph, Options);
    }

    [Theory]
    [InlineData("filter", "isUpperCase^", "( expected.")]
    [InlineData("filter", "isUpperCase(^", "Field name expected.")]
    [InlineData("filter", "isUpperCase(^ ", "Unexpected whitespace.")]
    [InlineData("filter", "isUpperCase(^)", "Field name expected.")]
    [InlineData("filter", "isUpperCase(^'a')", "Field name expected.")]
    [InlineData("filter", "isUpperCase(^some)", "Field 'some' does not exist on resource type 'blogs'.")]
    [InlineData("filter", "isUpperCase(^caption)", "Field 'caption' does not exist on resource type 'blogs'.")]
    [InlineData("filter", "isUpperCase(^null)", "Field name expected.")]
    [InlineData("filter", "isUpperCase(title)^)", "End of expression expected.")]
    [InlineData("filter", "isUpperCase(owner.preferences.^useDarkTheme)", "Attribute of type 'String' expected.")]
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
    [InlineData("filter", "isUpperCase(title)", null)]
    [InlineData("filter", "isUpperCase(owner.userName)", null)]
    [InlineData("filter", "has(posts,isUpperCase(author.userName))", null)]
    [InlineData("filter", "or(isUpperCase(title),isUpperCase(platformName))", null)]
    [InlineData("filter[posts]", "isUpperCase(author.userName)", "posts")]
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
