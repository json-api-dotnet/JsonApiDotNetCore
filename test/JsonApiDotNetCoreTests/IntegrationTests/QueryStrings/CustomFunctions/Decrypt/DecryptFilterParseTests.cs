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

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.Decrypt;

public sealed class DecryptFilterParseTests : BaseParseTests
{
    private readonly FilterQueryStringParameterReader _reader;

    public DecryptFilterParseTests()
    {
        using var serviceProvider = new ServiceContainer();
        var resourceFactory = new ResourceFactory(serviceProvider);
        var scopeParser = new QueryStringParameterScopeParser();
        var valueParser = new DecryptFilterParser(resourceFactory);

        _reader = new FilterQueryStringParameterReader(scopeParser, valueParser, Request, ResourceGraph, Options);
    }

    [Theory]
    [InlineData("filter", "equals(decrypt^", "( expected.")]
    [InlineData("filter", "equals(decrypt(^", "Field name expected.")]
    [InlineData("filter", "equals(decrypt(^ ", "Unexpected whitespace.")]
    [InlineData("filter", "equals(decrypt(^)", "Field name expected.")]
    [InlineData("filter", "equals(decrypt(^'a')", "Field name expected.")]
    [InlineData("filter", "equals(decrypt(^some)", "Field 'some' does not exist on resource type 'blogs'.")]
    [InlineData("filter", "equals(decrypt(^caption)", "Field 'caption' does not exist on resource type 'blogs'.")]
    [InlineData("filter", "equals(decrypt(^null)", "Field name expected.")]
    [InlineData("filter", "equals(decrypt(title)^)", ", expected.")]
    [InlineData("filter", "equals(decrypt(owner.preferences.^useDarkTheme)", "Attribute of type 'String' expected.")]
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
        error.Title.Should().Be("The specified filter is invalid.");
        error.Detail.Should().Be($"{errorMessage} {parameterValueSource}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be(parameterName);
    }

    [Theory]
    [InlineData("filter", "equals(decrypt(title),'secret')", null)]
    [InlineData("filter", "startsWith(decrypt(title),'secret')", null)]
    [InlineData("filter", "endsWith(decrypt(title),'secret')", null)]
    [InlineData("filter", "any(decrypt(title),'x','y')", null)]
    [InlineData("filter", "contains(decrypt(owner.userName),'secret')", null)]
    [InlineData("filter", "or(equals(decrypt(title),'one'),equals(decrypt(platformName),'two'))", null)]
    [InlineData("filter[posts]", "equals(decrypt(author.userName),'secret')", "posts")]
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
