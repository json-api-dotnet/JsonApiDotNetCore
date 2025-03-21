using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.QueryStringParameters;

public sealed class IncludeParseTests : BaseParseTests
{
    private readonly IncludeQueryStringParameterReader _reader;

    public IncludeParseTests()
    {
        var options = new JsonApiOptions();
        var parser = new IncludeParser(options);
        _reader = new IncludeQueryStringParameterReader(parser, Request, ResourceGraph);
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
    [InlineData("includes", "^ ", "Unexpected whitespace.")]
    [InlineData("includes", "^,", "Relationship name expected.")]
    [InlineData("includes", "posts,^", "Relationship name expected.")]
    [InlineData("includes", "posts.^", "Relationship name expected.")]
    [InlineData("includes", "posts^[", ", expected.")]
    [InlineData("includes", "^title", "Relationship 'title' does not exist on resource type 'blogs'.")]
    [InlineData("includes", "posts.comments.^publishTime,", "Relationship 'publishTime' does not exist on resource type 'comments'.")]
    [InlineData("includes", "owner.person.children.^unknown", "Relationship 'unknown' does not exist on resource type 'humans' or any of its derived types.")]
    [InlineData("includes", "owner.person.friends.^unknown", "Relationship 'unknown' does not exist on resource type 'humans' or any of its derived types.")]
    [InlineData("includes", "owner.person.sameGenderFriends.^unknown", "Relationship 'unknown' does not exist on any of the resource types 'men', 'women'.")]
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
        error.Title.Should().Be("The specified include is invalid.");
        error.Detail.Should().Be($"{errorMessage} {parameterValueSource}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be(parameterName);
    }

    [Theory]
    [InlineData("includes", "", "")]
    [InlineData("includes", "owner", "owner")]
    [InlineData("includes", "posts", "posts")]
    [InlineData("includes", "owner.posts", "owner.posts")]
    [InlineData("includes", "posts.author", "posts.author")]
    [InlineData("includes", "posts.comments", "posts.comments")]
    [InlineData("includes", "posts,posts.comments", "posts.comments")]
    [InlineData("includes", "posts,posts.labels,posts.comments", "posts.comments,posts.labels")]
    [InlineData("includes", "owner.person.children.husband", "owner.person.children.husband,owner.person.children.husband")]
    [InlineData("includes", "owner.person.wife,owner.person.husband", "owner.person.husband,owner.person.wife")]
    [InlineData("includes", "owner.person.father.children.wife", "owner.person.father.children.wife,owner.person.father.children.wife")]
    [InlineData("includes", "owner.person.friends", "owner.person.friends,owner.person.friends")]
    [InlineData("includes", "owner.person.friends.friends",
        "owner.person.friends.friends,owner.person.friends.friends,owner.person.friends.friends,owner.person.friends.friends")]
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
