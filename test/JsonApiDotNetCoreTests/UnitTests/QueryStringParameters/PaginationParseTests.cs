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

public sealed class PaginationParseTests : BaseParseTests
{
    private readonly PaginationQueryStringParameterReader _reader;

    public PaginationParseTests()
    {
        Options.DefaultPageSize = new PageSize(25);
        var parser = new PaginationParser();
        _reader = new PaginationQueryStringParameterReader(parser, Request, ResourceGraph, Options);
    }

    [Theory]
    [InlineData("page[size]", true)]
    [InlineData("page[number]", true)]
    [InlineData("page", false)]
    [InlineData("page[", false)]
    [InlineData("page[some]", false)]
    public void Reader_Supports_Parameter_Name(string parameterName, bool expectCanParse)
    {
        // Act
        bool canParse = _reader.CanRead(parameterName);

        // Assert
        canParse.Should().Be(expectCanParse);
    }

    [Theory]
    [InlineData(JsonApiQueryStringParameters.Page, false)]
    [InlineData(JsonApiQueryStringParameters.All, false)]
    [InlineData(JsonApiQueryStringParameters.None, true)]
    [InlineData(JsonApiQueryStringParameters.Sort, true)]
    public void Reader_Is_Enabled(JsonApiQueryStringParameters parametersDisabled, bool expectIsEnabled)
    {
        // Act
        bool isEnabled = _reader.IsEnabled(new DisableQueryStringAttribute(parametersDisabled));

        // Assert
        isEnabled.Should().Be(expectIsEnabled);
    }

    [Theory]
    [InlineData("^", "Number or relationship name expected.")]
    [InlineData("^.", "Number or relationship name expected.")]
    [InlineData("1,^", "Number or relationship name expected.")]
    [InlineData("^(", "Number or relationship name expected.")]
    [InlineData("^ ", "Unexpected whitespace.")]
    [InlineData("-^", "Digits expected.")]
    [InlineData("^-1", "Page number cannot be negative or zero.")]
    [InlineData("posts^", ": expected.")]
    [InlineData("posts:^", "Number expected.")]
    [InlineData("posts:^abc", "Number expected.")]
    [InlineData("1^(", ", expected.")]
    [InlineData("posts:-^abc", "Digits expected.")]
    [InlineData("posts:^-1", "Page number cannot be negative or zero.")]
    [InlineData("^unknown", "To-many relationship 'unknown' does not exist on resource type 'blogs'.")]
    [InlineData("^unknown.other", "Relationship 'unknown' does not exist on resource type 'blogs'.")]
    [InlineData("posts.^", "To-many relationship name expected.")]
    [InlineData("posts.^unknown", "To-many relationship 'unknown' does not exist on resource type 'blogPosts'.")]
    [InlineData("posts.^id", "To-many relationship 'id' does not exist on resource type 'blogPosts'.")]
    [InlineData("posts.^author", "To-many relationship 'author' does not exist on resource type 'blogPosts'.")]
    [InlineData("posts.author.^", "To-many relationship name expected.")]
    [InlineData("posts.comments.^id", "To-many relationship 'id' does not exist on resource type 'comments'.")]
    [InlineData("owner.person.^unknown", "To-many relationship 'unknown' does not exist on resource type 'humans' or any of its derived types.")]
    [InlineData("owner.person.^unknown.other", "Relationship 'unknown' does not exist on resource type 'humans' or any of its derived types.")]
    [InlineData("owner.person.^hasBeard", "To-many relationship 'hasBeard' does not exist on resource type 'humans' or any of its derived types.")]
    [InlineData("owner.person.^wife", "To-many relationship 'wife' does not exist on resource type 'humans' or any of its derived types.")]
    public void Reader_Read_Page_Number_Fails(string parameterValue, string errorMessage)
    {
        // Arrange
        var parameterValueSource = new MarkedText(parameterValue, '^');

        // Act
        Action action = () => _reader.Read("page[number]", parameterValueSource.Text);

        // Assert
        InvalidQueryStringParameterException exception = action.Should().ThrowExactly<InvalidQueryStringParameterException>().And;

        exception.ParameterName.Should().Be("page[number]");
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified pagination is invalid.");
        error.Detail.Should().Be($"{errorMessage} {parameterValueSource}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("page[number]");
    }

    [Theory]
    [InlineData("^", "Number or relationship name expected.")]
    [InlineData("^.", "Number or relationship name expected.")]
    [InlineData("1,^", "Number or relationship name expected.")]
    [InlineData("^(", "Number or relationship name expected.")]
    [InlineData("^ ", "Unexpected whitespace.")]
    [InlineData("-^", "Digits expected.")]
    [InlineData("^-1", "Page size cannot be negative.")]
    [InlineData("posts^", ": expected.")]
    [InlineData("posts:^", "Number expected.")]
    [InlineData("posts:^abc", "Number expected.")]
    [InlineData("1^(", ", expected.")]
    [InlineData("posts:-^abc", "Digits expected.")]
    [InlineData("posts:^-1", "Page size cannot be negative.")]
    [InlineData("^unknown", "To-many relationship 'unknown' does not exist on resource type 'blogs'.")]
    [InlineData("^unknown.other", "Relationship 'unknown' does not exist on resource type 'blogs'.")]
    [InlineData("posts.^", "To-many relationship name expected.")]
    [InlineData("posts.^unknown", "To-many relationship 'unknown' does not exist on resource type 'blogPosts'.")]
    [InlineData("posts.^id", "To-many relationship 'id' does not exist on resource type 'blogPosts'.")]
    [InlineData("posts.^author", "To-many relationship 'author' does not exist on resource type 'blogPosts'.")]
    [InlineData("posts.author.^", "To-many relationship name expected.")]
    [InlineData("posts.comments.^id", "To-many relationship 'id' does not exist on resource type 'comments'.")]
    [InlineData("owner.person.^unknown", "To-many relationship 'unknown' does not exist on resource type 'humans' or any of its derived types.")]
    [InlineData("owner.person.^unknown.other", "Relationship 'unknown' does not exist on resource type 'humans' or any of its derived types.")]
    [InlineData("owner.person.^hasBeard", "To-many relationship 'hasBeard' does not exist on resource type 'humans' or any of its derived types.")]
    [InlineData("owner.person.^wife", "To-many relationship 'wife' does not exist on resource type 'humans' or any of its derived types.")]
    public void Reader_Read_Page_Size_Fails(string parameterValue, string errorMessage)
    {
        // Arrange
        var parameterValueSource = new MarkedText(parameterValue, '^');

        // Act
        Action action = () => _reader.Read("page[size]", parameterValueSource.Text);

        // Assert
        InvalidQueryStringParameterException exception = action.Should().ThrowExactly<InvalidQueryStringParameterException>().And;

        exception.ParameterName.Should().Be("page[size]");
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified pagination is invalid.");
        error.Detail.Should().Be($"{errorMessage} {parameterValueSource}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("page[size]");
    }

    [Theory]
    [InlineData(null, "5", "", "Page number: 1, size: 5")]
    [InlineData("2", null, "", "Page number: 2, size: 25")]
    [InlineData("2", "5", "", "Page number: 2, size: 5")]
    [InlineData("posts:4", "posts:2", "|posts", "Page number: 1, size: 25|Page number: 4, size: 2")]
    [InlineData("posts:4", "5", "|posts", "Page number: 1, size: 5|Page number: 4, size: 25")]
    [InlineData("4", "posts:5", "|posts", "Page number: 4, size: 25|Page number: 1, size: 5")]
    [InlineData("3,owner.posts:4", "20,owner.posts:10", "|owner.posts", "Page number: 3, size: 20|Page number: 4, size: 10")]
    [InlineData("posts:4,3", "posts:10,20", "|posts", "Page number: 3, size: 20|Page number: 4, size: 10")]
    [InlineData("posts:4,posts.comments:5,3", "posts:10,posts.comments:15,20", "|posts|posts.comments",
        "Page number: 3, size: 20|Page number: 4, size: 10|Page number: 5, size: 15")]
    [InlineData("owner.person.wife.children:5,2", "owner.person.husband.children:8,3", "|owner.person.husband.children|owner.person.wife.children",
        "Page number: 2, size: 3|Page number: 1, size: 8|Page number: 5, size: 25")]
    public void Reader_Read_Pagination_Succeeds(string? pageNumber, string? pageSize, string scopeTreesExpected, string valueTreesExpected)
    {
        // Act
        if (pageNumber != null)
        {
            _reader.Read("page[number]", pageNumber);
        }

        if (pageSize != null)
        {
            _reader.Read("page[size]", pageSize);
        }

        IReadOnlyCollection<ExpressionInScope> constraints = _reader.GetConstraints();

        // Assert
        string[] scopeTreesExpectedArray = scopeTreesExpected.Split("|");
        ResourceFieldChainExpression?[] scopeTrees = constraints.Select(expressionInScope => expressionInScope.Scope).ToArray();

        scopeTrees.Should().HaveSameCount(scopeTreesExpectedArray);
        scopeTrees.Select(tree => tree?.ToString() ?? "").Should().BeEquivalentTo(scopeTreesExpectedArray, options => options.WithStrictOrdering());

        string[] valueTreesExpectedArray = valueTreesExpected.Split("|");
        QueryExpression[] valueTrees = constraints.Select(expressionInScope => expressionInScope.Expression).ToArray();

        valueTrees.Should().HaveSameCount(valueTreesExpectedArray);
        valueTrees.Select(tree => tree.ToString()).Should().BeEquivalentTo(valueTreesExpectedArray, options => options.WithStrictOrdering());
    }
}
