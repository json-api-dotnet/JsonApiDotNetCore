using System;
using System.Linq;
using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.QueryStrings.Internal;
using Xunit;

namespace UnitTests.QueryStringParameters
{
    public sealed class PaginationParseTests : BaseParseTests
    {
        private readonly IPaginationQueryStringParameterReader _reader;

        public PaginationParseTests()
        {
            Options.DefaultPageSize = new PageSize(25);
            _reader = new PaginationQueryStringParameterReader(Request, ResourceGraph, Options);
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
            var canParse = _reader.CanRead(parameterName);
            
            // Assert
            canParse.Should().Be(expectCanParse);
        }

        [Theory]
        [InlineData(StandardQueryStringParameters.Page, false)]
        [InlineData(StandardQueryStringParameters.All, false)]
        [InlineData(StandardQueryStringParameters.None, true)]
        [InlineData(StandardQueryStringParameters.Sort, true)]
        public void Reader_Is_Enabled(StandardQueryStringParameters parametersDisabled, bool expectIsEnabled)
        {
            // Act
            var isEnabled = _reader.IsEnabled(new DisableQueryStringAttribute(parametersDisabled));
            
            // Assert
            isEnabled.Should().Be(expectIsEnabled);
        }

        [Theory]
        [InlineData("", "Number or relationship name expected.")]
        [InlineData("1,", "Number or relationship name expected.")]
        [InlineData("(", "Number or relationship name expected.")]
        [InlineData(" ", "Unexpected whitespace.")]
        [InlineData("-", "Digits expected.")]
        [InlineData("-1", "Page number cannot be negative or zero.")]
        [InlineData("articles", ": expected.")]
        [InlineData("articles:", "Number expected.")]
        [InlineData("articles:abc", "Number expected.")]
        [InlineData("1(", ", expected.")]
        [InlineData("articles:-abc", "Digits expected.")]
        [InlineData("articles:-1", "Page number cannot be negative or zero.")]
        [InlineData("articles.id", "Relationship 'id' in 'articles.id' does not exist on resource 'articles'.")]
        [InlineData("articles.tags.id", "Relationship 'id' in 'articles.tags.id' does not exist on resource 'tags'.")]
        [InlineData("articles.author", "Relationship 'author' in 'articles.author' must be a to-many relationship on resource 'articles'.")]
        [InlineData("something", "Relationship 'something' does not exist on resource 'blogs'.")]
        public void Reader_Read_Page_Number_Fails(string parameterValue, string errorMessage)
        {
            // Act
            Action action = () => _reader.Read("page[number]", parameterValue);

            // Assert
            var exception = action.Should().ThrowExactly<InvalidQueryStringParameterException>().And;

            exception.QueryParameterName.Should().Be("page[number]");
            exception.Errors.Should().HaveCount(1);
            exception.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            exception.Errors[0].Title.Should().Be("The specified paging is invalid.");
            exception.Errors[0].Detail.Should().Be(errorMessage);
            exception.Errors[0].Source.Parameter.Should().Be("page[number]");
        }

        [Theory]
        [InlineData("", "Number or relationship name expected.")]
        [InlineData("1,", "Number or relationship name expected.")]
        [InlineData("(", "Number or relationship name expected.")]
        [InlineData(" ", "Unexpected whitespace.")]
        [InlineData("-", "Digits expected.")]
        [InlineData("-1", "Page size cannot be negative.")]
        [InlineData("articles", ": expected.")]
        [InlineData("articles:", "Number expected.")]
        [InlineData("articles:abc", "Number expected.")]
        [InlineData("1(", ", expected.")]
        [InlineData("articles:-abc", "Digits expected.")]
        [InlineData("articles:-1", "Page size cannot be negative.")]
        [InlineData("articles.id", "Relationship 'id' in 'articles.id' does not exist on resource 'articles'.")]
        [InlineData("articles.tags.id", "Relationship 'id' in 'articles.tags.id' does not exist on resource 'tags'.")]
        [InlineData("articles.author", "Relationship 'author' in 'articles.author' must be a to-many relationship on resource 'articles'.")]
        [InlineData("something", "Relationship 'something' does not exist on resource 'blogs'.")]
        public void Reader_Read_Page_Size_Fails(string parameterValue, string errorMessage)
        {
            // Act
            Action action = () => _reader.Read("page[size]", parameterValue);

            // Assert
            var exception = action.Should().ThrowExactly<InvalidQueryStringParameterException>().And;

            exception.QueryParameterName.Should().Be("page[size]");
            exception.Errors.Should().HaveCount(1);
            exception.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            exception.Errors[0].Title.Should().Be("The specified paging is invalid.");
            exception.Errors[0].Detail.Should().Be(errorMessage);
            exception.Errors[0].Source.Parameter.Should().Be("page[size]");
        }

        [Theory]
        [InlineData(null, "5", "", "Page number: 1, size: 5")]
        [InlineData("2", null, "", "Page number: 2, size: 25")]
        [InlineData("2", "5", "", "Page number: 2, size: 5")]
        [InlineData("articles:4", "articles:2", "|articles", "Page number: 1, size: 25|Page number: 4, size: 2")]
        [InlineData("articles:4", "5", "|articles", "Page number: 1, size: 5|Page number: 4, size: 25")]
        [InlineData("4", "articles:5", "|articles", "Page number: 4, size: 25|Page number: 1, size: 5")]
        [InlineData("3,owner.articles:4", "20,owner.articles:10", "|owner.articles", "Page number: 3, size: 20|Page number: 4, size: 10")]
        [InlineData("articles:4,3", "articles:10,20", "|articles", "Page number: 3, size: 20|Page number: 4, size: 10")]
        [InlineData("articles:4,articles.revisions:5,3", "articles:10,articles.revisions:15,20", "|articles|articles.revisions", "Page number: 3, size: 20|Page number: 4, size: 10|Page number: 5, size: 15")]
        public void Reader_Read_Pagination_Succeeds(string pageNumber, string pageSize, string scopeTreesExpected, string valueTreesExpected)
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

            var constraints = _reader.GetConstraints();

            // Assert
            var scopeTreesExpectedArray = scopeTreesExpected.Split("|");
            var scopeTrees = constraints.Select(x => x.Scope).ToArray();

            scopeTrees.Should().HaveSameCount(scopeTreesExpectedArray);
            scopeTrees.Select(tree => tree?.ToString() ?? "").Should().BeEquivalentTo(scopeTreesExpectedArray, options => options.WithStrictOrdering());

            var valueTreesExpectedArray = valueTreesExpected.Split("|");
            var valueTrees = constraints.Select(x => x.Expression).ToArray();

            valueTrees.Should().HaveSameCount(valueTreesExpectedArray);
            valueTrees.Select(tree => tree.ToString()).Should().BeEquivalentTo(valueTreesExpectedArray, options => options.WithStrictOrdering());
        }
    }
}
