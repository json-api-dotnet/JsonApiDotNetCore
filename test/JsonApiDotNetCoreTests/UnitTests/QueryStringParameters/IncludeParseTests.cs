using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.QueryStrings.Internal;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.QueryStringParameters;

public sealed class IncludeParseTests : BaseParseTests
{
    private readonly IncludeQueryStringParameterReader _reader;

    public IncludeParseTests()
    {
        _reader = new IncludeQueryStringParameterReader(Request, ResourceGraph, new JsonApiOptions());
    }

    [Theory]
    [InlineData("include", true)]
    [InlineData("include[some]", false)]
    [InlineData("includes", false)]
    public void Reader_Supports_Parameter_Name(string parameterName, bool expectCanParse)
    {
        // Act
        bool canParse = _reader.CanRead(parameterName);

        // Assert
        canParse.Should().Be(expectCanParse);
    }

    [Theory]
    [InlineData(JsonApiQueryStringParameters.Include, false)]
    [InlineData(JsonApiQueryStringParameters.All, false)]
    [InlineData(JsonApiQueryStringParameters.None, true)]
    [InlineData(JsonApiQueryStringParameters.Filter, true)]
    public void Reader_Is_Enabled(JsonApiQueryStringParameters parametersDisabled, bool expectIsEnabled)
    {
        // Act
        bool isEnabled = _reader.IsEnabled(new DisableQueryStringAttribute(parametersDisabled));

        // Assert
        isEnabled.Should().Be(expectIsEnabled);
    }

    [Theory]
    [InlineData("includes", "", "Relationship name expected.")]
    [InlineData("includes", " ", "Unexpected whitespace.")]
    [InlineData("includes", ",", "Relationship name expected.")]
    [InlineData("includes", "posts,", "Relationship name expected.")]
    [InlineData("includes", "posts[", ", expected.")]
    [InlineData("includes", "title", "Relationship 'title' does not exist on resource type 'blogs'.")]
    [InlineData("includes", "posts.comments.publishTime,",
        "Relationship 'publishTime' in 'posts.comments.publishTime' does not exist on resource type 'comments'.")]
    public void Reader_Read_Fails(string parameterName, string parameterValue, string errorMessage)
    {
        // Act
        Action action = () => _reader.Read(parameterName, parameterValue);

        // Assert
        InvalidQueryStringParameterException exception = action.Should().ThrowExactly<InvalidQueryStringParameterException>().And;

        exception.ParameterName.Should().Be(parameterName);
        exception.Errors.ShouldHaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified include is invalid.");
        error.Detail.Should().Be(errorMessage);
        error.Source.ShouldNotBeNull();
        error.Source.Parameter.Should().Be(parameterName);
    }

    [Theory]
    [InlineData("includes", "owner", "owner")]
    [InlineData("includes", "posts", "posts")]
    [InlineData("includes", "owner.posts", "owner.posts")]
    [InlineData("includes", "posts.author", "posts.author")]
    [InlineData("includes", "posts.comments", "posts.comments")]
    [InlineData("includes", "posts,posts.comments", "posts.comments")]
    [InlineData("includes", "posts,posts.labels,posts.comments", "posts.comments,posts.labels")]
    public void Reader_Read_Succeeds(string parameterName, string parameterValue, string valueExpected)
    {
        // Act
        _reader.Read(parameterName, parameterValue);

        IReadOnlyCollection<ExpressionInScope> constraints = _reader.GetConstraints();

        // Assert
        ResourceFieldChainExpression? scope = constraints.Select(expressionInScope => expressionInScope.Scope).Single();
        scope.Should().BeNull();

        QueryExpression value = constraints.Select(expressionInScope => expressionInScope.Expression).Single();
        value.ToString().Should().Be(valueExpected);
    }
}
