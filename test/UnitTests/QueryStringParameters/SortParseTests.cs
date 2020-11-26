using System;
using System.Linq;
using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.QueryStrings.Internal;
using Xunit;

namespace UnitTests.QueryStringParameters
{
    public sealed class SortParseTests : BaseParseTests
    {
        private readonly SortQueryStringParameterReader _reader;

        public SortParseTests()
        {
            _reader = new SortQueryStringParameterReader(Request, ResourceGraph);
        }

        [Theory]
        [InlineData("sort", true)]
        [InlineData("sort[articles]", true)]
        [InlineData("sort[articles.revisions]", true)]
        [InlineData("sorting", false)]
        [InlineData("sort[", false)]
        [InlineData("sort]", false)]
        public void Reader_Supports_Parameter_Name(string parameterName, bool expectCanParse)
        {
            // Act
            var canParse = _reader.CanRead(parameterName);

            // Assert
            canParse.Should().Be(expectCanParse);
        }

        [Theory]
        [InlineData(StandardQueryStringParameters.Sort, false)]
        [InlineData(StandardQueryStringParameters.All, false)]
        [InlineData(StandardQueryStringParameters.None, true)]
        [InlineData(StandardQueryStringParameters.Filter, true)]
        public void Reader_Is_Enabled(StandardQueryStringParameters parametersDisabled, bool expectIsEnabled)
        {
            // Act
            var isEnabled = _reader.IsEnabled(new DisableQueryStringAttribute(parametersDisabled));

            // Assert
            isEnabled.Should().Be(expectIsEnabled);
        }

        [Theory]
        [InlineData("sort[", "id", "Field name expected.")]
        [InlineData("sort[abc.def]", "id", "Relationship 'abc' in 'abc.def' does not exist on resource 'blogs'.")]
        [InlineData("sort[articles.author]", "id", "Relationship 'author' in 'articles.author' must be a to-many relationship on resource 'articles'.")]
        [InlineData("sort", "", "-, count function or field name expected.")]
        [InlineData("sort", " ", "Unexpected whitespace.")]
        [InlineData("sort", "-", "Count function or field name expected.")]
        [InlineData("sort", "abc", "Attribute 'abc' does not exist on resource 'blogs'.")]
        [InlineData("sort[articles]", "author", "Attribute 'author' does not exist on resource 'articles'.")]
        [InlineData("sort[articles]", "author.livingAddress", "Attribute 'livingAddress' in 'author.livingAddress' does not exist on resource 'authors'.")]
        [InlineData("sort", "-count", "( expected.")]
        [InlineData("sort", "count", "( expected.")]
        [InlineData("sort", "count(articles", ") expected.")]
        [InlineData("sort", "count(", "Field name expected.")]
        [InlineData("sort", "count(-abc)", "Field name expected.")]
        [InlineData("sort", "count(abc)", "Relationship 'abc' does not exist on resource 'blogs'.")]
        [InlineData("sort", "count(id)", "Relationship 'id' does not exist on resource 'blogs'.")]
        [InlineData("sort[articles]", "count(author)", "Relationship 'author' must be a to-many relationship on resource 'articles'.")]
        [InlineData("sort[articles]", "caption,", "-, count function or field name expected.")]
        [InlineData("sort[articles]", "caption:", ", expected.")]
        [InlineData("sort[articles]", "caption,-", "Count function or field name expected.")]
        public void Reader_Read_Fails(string parameterName, string parameterValue, string errorMessage)
        {
            // Act
            Action action = () => _reader.Read(parameterName, parameterValue);

            // Assert
            var exception = action.Should().ThrowExactly<InvalidQueryStringParameterException>().And;

            exception.QueryParameterName.Should().Be(parameterName);
            exception.Errors.Should().HaveCount(1);
            exception.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            exception.Errors[0].Title.Should().Be("The specified sort is invalid.");
            exception.Errors[0].Detail.Should().Be(errorMessage);
            exception.Errors[0].Source.Parameter.Should().Be(parameterName);
        }

        [Theory]
        [InlineData("sort", "id", null, "id")]
        [InlineData("sort", "count(articles),-id", null, "count(articles),-id")]
        [InlineData("sort", "-count(articles),id", null, "-count(articles),id")]
        [InlineData("sort[articles]", "count(revisions),-id", "articles", "count(revisions),-id")]
        [InlineData("sort[owner.articles]", "-caption", "owner.articles", "-caption")]
        [InlineData("sort[articles]", "author.firstName", "articles", "author.firstName")]
        [InlineData("sort[articles]", "-caption,-author.firstName", "articles", "-caption,-author.firstName")]
        [InlineData("sort[articles]", "caption,author.firstName,-id", "articles", "caption,author.firstName,-id")]
        [InlineData("sort[articles.tags]", "id,name", "articles.tags", "id,name")]
        [InlineData("sort[articles.revisions]", "-publishTime,author.lastName,author.livingAddress.country.name", "articles.revisions", "-publishTime,author.lastName,author.livingAddress.country.name")]
        public void Reader_Read_Succeeds(string parameterName, string parameterValue, string scopeExpected, string valueExpected)
        {
            // Act
            _reader.Read(parameterName, parameterValue);

            var constraints = _reader.GetConstraints();

            // Assert
            var scope = constraints.Select(x => x.Scope).Single();
            scope?.ToString().Should().Be(scopeExpected);

            var value = constraints.Select(x => x.Expression).Single();
            value.ToString().Should().Be(valueExpected);
        }
    }
}
