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
    [InlineData("sort[^", "Field name expected.")]
    [InlineData("sort[^abc.def]", "Field 'abc' does not exist on resource type 'blogs'.")]
    [InlineData("sort[posts.^caption]",
        "Field chain on resource type 'blogs' failed to match the pattern: zero or more relationships, followed by a to-many relationship. " +
        "Relationship on resource type 'blogPosts' expected.")]
    [InlineData("sort[posts.author^]",
        "Field chain on resource type 'blogs' failed to match the pattern: zero or more relationships, followed by a to-many relationship. " +
        "Relationship on resource type 'webAccounts' expected.")]
    public void Reader_Read_ParameterName_Fails(string parameterName, string errorMessage)
    {
        // Arrange
        var parameterNameSource = new MarkedText(parameterName, '^');

        // Act
        Action action = () => _reader.Read(parameterNameSource.Text, " ");

        // Assert
        InvalidQueryStringParameterException exception = action.Should().ThrowExactly<InvalidQueryStringParameterException>().And;

        exception.ParameterName.Should().Be(parameterNameSource.Text);
        exception.Errors.ShouldHaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified sort is invalid.");
        error.Detail.Should().Be($"{errorMessage} {parameterNameSource}");
        error.Source.ShouldNotBeNull();
        error.Source.Parameter.Should().Be(parameterNameSource.Text);
    }

    [Theory]
    [InlineData("sort", "^", "-, count function or field name expected.")]
    [InlineData("sort", "^ ", "Unexpected whitespace.")]
    [InlineData("sort", "-^", "Count function or field name expected.")]
    [InlineData("sort", "^abc", "Field 'abc' does not exist on resource type 'blogs'.")]
    [InlineData("sort[posts]", "author^",
        "Field chain on resource type 'blogPosts' failed to match the pattern: zero or more to-one relationships, followed by an attribute. " +
        "To-one relationship or attribute on resource type 'webAccounts' expected.")]
    [InlineData("sort[posts]", "author.^livingAddress", "Field 'livingAddress' does not exist on resource type 'webAccounts'.")]
    [InlineData("sort", "-count^", "( expected.")]
    [InlineData("sort", "count^", "( expected.")]
    [InlineData("sort", "count(posts^", ") expected.")]
    [InlineData("sort", "count(^", "Field name expected.")]
    [InlineData("sort", "count(^-abc)", "Field name expected.")]
    [InlineData("sort", "count(^abc)", "Field 'abc' does not exist on resource type 'blogs'.")]
    [InlineData("sort", "count(^id)",
        "Field chain on resource type 'blogs' failed to match the pattern: zero or more to-one relationships, followed by a to-many relationship. " +
        "Relationship on resource type 'blogs' expected.")]
    [InlineData("sort[posts]", "count(author^)",
        "Field chain on resource type 'blogPosts' failed to match the pattern: zero or more to-one relationships, followed by a to-many relationship. " +
        "Relationship on resource type 'webAccounts' expected.")]
    [InlineData("sort[posts]", "caption,^", "-, count function or field name expected.")]
    [InlineData("sort[posts]", "caption^:", ", expected.")]
    [InlineData("sort[posts]", "caption,-^", "Count function or field name expected.")]
    [InlineData("sort[posts.contributors]", "^some", "Field 'some' does not exist on resource type 'humans' or any of its derived types.")]
    [InlineData("sort[posts.contributors]", "wife.father.^some", "Field 'some' does not exist on resource type 'men'.")]
    [InlineData("sort[posts.contributors]", "count(^some)", "Field 'some' does not exist on resource type 'humans' or any of its derived types.")]
    [InlineData("sort[posts.contributors]", "count(wife.^some)", "Field 'some' does not exist on resource type 'women'.")]
    [InlineData("sort[posts.contributors]", "^age", "Field 'age' is defined on multiple types that derive from resource type 'humans'.")]
    [InlineData("sort[posts.contributors]", "count(^friends)", "Field 'friends' is defined on multiple types that derive from resource type 'humans'.")]
    public void Reader_Read_ParameterValue_Fails(string parameterName, string parameterValue, string errorMessage)
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
        error.Title.Should().Be("The specified sort is invalid.");
        error.Detail.Should().Be($"{errorMessage} {parameterValueSource}");
        error.Source.ShouldNotBeNull();
        error.Source.Parameter.Should().Be(parameterName);
    }

    [Theory]
    [InlineData("sort", "id", null)]
    [InlineData("sort", "count(posts),-id", null)]
    [InlineData("sort", "-count(posts),id", null)]
    [InlineData("sort[posts]", "count(comments),-id", "posts")]
    [InlineData("sort[owner.posts]", "-caption", "owner.posts")]
    [InlineData("sort[posts]", "author.userName", "posts")]
    [InlineData("sort[posts]", "-caption,-author.userName", "posts")]
    [InlineData("sort[posts]", "caption,author.userName,-id", "posts")]
    [InlineData("sort[posts.labels]", "id,name", "posts.labels")]
    [InlineData("sort[posts.comments]", "-createdAt,author.displayName,author.preferences.useDarkTheme", "posts.comments")]
    [InlineData("sort[posts.contributors]", "name,-maidenName,hasBeard", "posts.contributors")]
    [InlineData("sort[posts.contributors]", "husband.hasBeard,wife.maidenName", "posts.contributors")]
    [InlineData("sort[posts.contributors]", "count(wife.husband.drinkingBuddies)", "posts.contributors")]
    [InlineData("sort[posts.contributors]", "wife.age", "posts.contributors")]
    [InlineData("sort[posts.contributors]", "count(father.friends)", "posts.contributors")]
    public void Reader_Read_Succeeds(string parameterName, string parameterValue, string scopeExpected)
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
