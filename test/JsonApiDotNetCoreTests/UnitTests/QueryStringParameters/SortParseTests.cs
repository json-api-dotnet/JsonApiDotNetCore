using System.Net;
using FluentAssertions;
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

public sealed class SortParseTests : BaseParseTests
{
    private readonly SortQueryStringParameterReader _reader;

    public SortParseTests()
    {
        _reader = new SortQueryStringParameterReader(Request, ResourceGraph);
    }

    [Theory]
    [InlineData("sort", true)]
    [InlineData("sort[posts]", true)]
    [InlineData("sort[posts.comments]", true)]
    [InlineData("sorting", false)]
    [InlineData("sort[", false)]
    [InlineData("sort]", false)]
    public void Reader_Supports_Parameter_Name(string parameterName, bool expectCanParse)
    {
        // Act
        bool canParse = _reader.CanRead(parameterName);

        // Assert
        canParse.Should().Be(expectCanParse);
    }

    [Theory]
    [InlineData(JsonApiQueryStringParameters.Sort, false)]
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
    [InlineData("sort[", "id", "Field name expected.")]
    [InlineData("sort[abc.def]", "id", "Relationship 'abc' in 'abc.def' does not exist on resource type 'blogs'.")]
    [InlineData("sort[posts.author]", "id", "Relationship 'author' in 'posts.author' must be a to-many relationship on resource type 'blogPosts'.")]
    [InlineData("sort", "", "-, count function or field name expected.")]
    [InlineData("sort", " ", "Unexpected whitespace.")]
    [InlineData("sort", "-", "Count function or field name expected.")]
    [InlineData("sort", "abc", "Attribute 'abc' does not exist on resource type 'blogs'.")]
    [InlineData("sort[posts]", "author", "Attribute 'author' does not exist on resource type 'blogPosts'.")]
    [InlineData("sort[posts]", "author.livingAddress", "Attribute 'livingAddress' in 'author.livingAddress' does not exist on resource type 'webAccounts'.")]
    [InlineData("sort", "-count", "( expected.")]
    [InlineData("sort", "count", "( expected.")]
    [InlineData("sort", "count(posts", ") expected.")]
    [InlineData("sort", "count(", "Field name expected.")]
    [InlineData("sort", "count(-abc)", "Field name expected.")]
    [InlineData("sort", "count(abc)", "Relationship 'abc' does not exist on resource type 'blogs'.")]
    [InlineData("sort", "count(id)", "Relationship 'id' does not exist on resource type 'blogs'.")]
    [InlineData("sort[posts]", "count(author)", "Relationship 'author' must be a to-many relationship on resource type 'blogPosts'.")]
    [InlineData("sort[posts]", "caption,", "-, count function or field name expected.")]
    [InlineData("sort[posts]", "caption:", ", expected.")]
    [InlineData("sort[posts]", "caption,-", "Count function or field name expected.")]
    [InlineData("sort[posts.contributors]", "some", "Attribute 'some' does not exist on resource type 'humans' or any of its derived types.")]
    [InlineData("sort[posts.contributors]", "wife.father.some", "Attribute 'some' in 'wife.father.some' does not exist on resource type 'men'.")]
    [InlineData("sort[posts.contributors]", "count(some)", "Relationship 'some' does not exist on resource type 'humans' or any of its derived types.")]
    [InlineData("sort[posts.contributors]", "count(wife.some)", "Relationship 'some' in 'wife.some' does not exist on resource type 'women'.")]
    [InlineData("sort[posts.contributors]", "age", "Attribute 'age' is defined on multiple derived types.")]
    [InlineData("sort[posts.contributors]", "count(friends)", "Relationship 'friends' is defined on multiple derived types.")]
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
        error.Title.Should().Be("The specified sort is invalid.");
        error.Detail.Should().Be(errorMessage);
        error.Source.ShouldNotBeNull();
        error.Source.Parameter.Should().Be(parameterName);
    }

    [Theory]
    [InlineData("sort", "id", null, "id")]
    [InlineData("sort", "count(posts),-id", null, "count(posts),-id")]
    [InlineData("sort", "-count(posts),id", null, "-count(posts),id")]
    [InlineData("sort[posts]", "count(comments),-id", "posts", "count(comments),-id")]
    [InlineData("sort[owner.posts]", "-caption", "owner.posts", "-caption")]
    [InlineData("sort[posts]", "author.userName", "posts", "author.userName")]
    [InlineData("sort[posts]", "-caption,-author.userName", "posts", "-caption,-author.userName")]
    [InlineData("sort[posts]", "caption,author.userName,-id", "posts", "caption,author.userName,-id")]
    [InlineData("sort[posts.labels]", "id,name", "posts.labels", "id,name")]
    [InlineData("sort[posts.comments]", "-createdAt,author.displayName,author.preferences.useDarkTheme", "posts.comments",
        "-createdAt,author.displayName,author.preferences.useDarkTheme")]
    [InlineData("sort[posts.contributors]", "name,-maidenName,hasBeard", "posts.contributors", "name,-maidenName,hasBeard")]
    [InlineData("sort[posts.contributors]", "husband.hasBeard,wife.maidenName", "posts.contributors", "husband.hasBeard,wife.maidenName")]
    [InlineData("sort[posts.contributors]", "count(wife.husband.drinkingBuddies)", "posts.contributors", "count(wife.husband.drinkingBuddies)")]
    [InlineData("sort[posts.contributors]", "wife.age", "posts.contributors", "wife.age")]
    [InlineData("sort[posts.contributors]", "count(father.friends)", "posts.contributors", "count(father.friends)")]
    public void Reader_Read_Succeeds(string parameterName, string parameterValue, string scopeExpected, string valueExpected)
    {
        // Act
        _reader.Read(parameterName, parameterValue);

        IReadOnlyCollection<ExpressionInScope> constraints = _reader.GetConstraints();

        // Assert
        ResourceFieldChainExpression? scope = constraints.Select(expressionInScope => expressionInScope.Scope).Single();
        scope?.ToString().Should().Be(scopeExpected);

        QueryExpression value = constraints.Select(expressionInScope => expressionInScope.Expression).Single();
        value.ToString().Should().Be(valueExpected);
    }
}
