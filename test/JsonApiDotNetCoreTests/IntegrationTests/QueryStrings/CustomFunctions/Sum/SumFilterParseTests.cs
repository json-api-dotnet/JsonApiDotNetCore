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

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.Sum;

public sealed class SumFilterParseTests : BaseParseTests
{
    private readonly FilterQueryStringParameterReader _reader;

    public SumFilterParseTests()
    {
        var resourceFactory = new ResourceFactory(new ServiceContainer());
        var scopeParser = new QueryStringParameterScopeParser();
        var valueParser = new SumFilterParser(resourceFactory);

        _reader = new FilterQueryStringParameterReader(scopeParser, valueParser, Request, ResourceGraph, Options);
    }

    [Theory]
    [InlineData("filter", "equals(sum^", "( expected.")]
    [InlineData("filter", "equals(sum(^", "To-many relationship expected.")]
    [InlineData("filter", "equals(sum(^ ", "Unexpected whitespace.")]
    [InlineData("filter", "equals(sum(^)", "To-many relationship expected.")]
    [InlineData("filter", "equals(sum(^'a')", "To-many relationship expected.")]
    [InlineData("filter", "equals(sum(^null)", "To-many relationship expected.")]
    [InlineData("filter", "equals(sum(^some)", "Field 'some' does not exist on resource type 'blogs'.")]
    [InlineData("filter", "equals(sum(^title)",
        "Field chain on resource type 'blogs' failed to match the pattern: a to-many relationship. " +
        "To-many relationship on resource type 'blogs' expected.")]
    [InlineData("filter", "equals(sum(posts^))", ", expected.")]
    [InlineData("filter", "equals(sum(posts,^))", "Field name expected.")]
    [InlineData("filter", "equals(sum(posts,author^))",
        "Field chain on resource type 'blogPosts' failed to match the pattern: zero or more to-one relationships, followed by an attribute. " +
        "To-one relationship or attribute on resource type 'webAccounts' expected.")]
    [InlineData("filter", "equals(sum(posts,^url))", "Attribute of a numeric type expected.")]
    [InlineData("filter", "equals(sum(posts,^has(labels)))", "Function that returns a numeric type expected.")]
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
    [InlineData("filter", "has(posts,greaterThan(sum(comments,numStars),'5'))", null)]
    [InlineData("filter[posts]", "equals(sum(comments,numStars),'11')", "posts")]
    [InlineData("filter[posts]", "equals(sum(labels,count(posts)),'8')", "posts")]
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
