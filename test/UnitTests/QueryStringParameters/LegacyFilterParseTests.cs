using System;
using System.ComponentModel.Design;
using System.Linq;
using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Exceptions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.QueryStrings;
using JsonApiDotNetCoreExample.Models;
using Xunit;

namespace UnitTests.QueryStringParameters
{
    public sealed class LegacyFilterParseTests : ParseTestsBase
    {
        private readonly FilterQueryStringParameterReader _reader;

        public LegacyFilterParseTests()
        {
            Options.EnableLegacyFilterNotation = true;

            CurrentRequest.PrimaryResource = ResourceGraph.GetResourceContext<Article>();

            var resourceFactory = new ResourceFactory(new ServiceContainer());
            _reader = new FilterQueryStringParameterReader(CurrentRequest, ResourceGraph, resourceFactory, Options);
        }

        [Theory]
        [InlineData("filter", "some", "Expected field name between brackets in filter parameter name.")]
        [InlineData("filter[", "some", "Expected field name between brackets in filter parameter name.")]
        [InlineData("filter[]", "some", "Expected field name between brackets in filter parameter name.")]
        [InlineData("filter[.]", "some", "Relationship '' in '.' does not exist on resource 'articles'.")]
        [InlineData("filter[some]", "other", "Field 'some' does not exist on resource 'articles'.")]
        [InlineData("filter[author]", "some", "Attribute 'author' does not exist on resource 'articles'.")]
        [InlineData("filter[author.articles]", "some", "Field 'articles' in 'author.articles' must be an attribute or a to-one relationship on resource 'authors'.")]
        [InlineData("filter[unknown.id]", "some", "Relationship 'unknown' in 'unknown.id' does not exist on resource 'articles'.")]
        [InlineData("filter[author]", " ", "Unexpected whitespace.")]
        [InlineData("filter", "expr:equals(some,'other')", "Field 'some' does not exist on resource 'articles'.")]
        [InlineData("filter", "expr:equals(author,'Joe')", "Attribute 'author' does not exist on resource 'articles'.")]
        [InlineData("filter", "expr:has(author)", "Relationship 'author' must be a to-many relationship on resource 'articles'.")]
        [InlineData("filter", "expr:equals(count(author),'1')", "Relationship 'author' must be a to-many relationship on resource 'articles'.")]
        public void Reader_Read_Fails(string parameterName, string parameterValue, string errorMessage)
        {
            // Act
            Action action = () => _reader.Read(parameterName, parameterValue);

            // Assert
            var exception = action.Should().ThrowExactly<InvalidQueryStringParameterException>().And;

            exception.QueryParameterName.Should().Be(parameterName);
            exception.Error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            exception.Error.Title.Should().Be("The specified filter is invalid.");
            exception.Error.Detail.Should().Be(errorMessage);
            exception.Error.Source.Parameter.Should().Be(parameterName);
        }

        [Theory]
        [InlineData("filter[caption]", "Brian O'Quote", "equals(caption,'Brian O''Quote')")]
        [InlineData("filter[caption]", "using,comma", "equals(caption,'using,comma')")]
        [InlineData("filter[caption]", "am&per-sand", "equals(caption,'am&per-sand')")]
        [InlineData("filter[caption]", "2017-08-15T22:43:47.0156350-05:00", "equals(caption,'2017-08-15T22:43:47.0156350-05:00')")]
        [InlineData("filter[caption]", "eq:1", "equals(caption,'1')")]
        [InlineData("filter[caption]", "lt:2", "lessThan(caption,'2')")]
        [InlineData("filter[caption]", "gt:3", "greaterThan(caption,'3')")]
        [InlineData("filter[caption]", "le:4", "lessOrEqual(caption,'4')")]
        [InlineData("filter[caption]", "le:2017-08-15T22:43:47.0156350-05:00", "lessOrEqual(caption,'2017-08-15T22:43:47.0156350-05:00')")]
        [InlineData("filter[caption]", "ge:5", "greaterOrEqual(caption,'5')")]
        [InlineData("filter[caption]", "like:that", "contains(caption,'that')")]
        [InlineData("filter[caption]", "ne:1", "not(equals(caption,'1'))")]
        [InlineData("filter[caption]", "in:first,second", "any(caption,'first','second')")]
        [InlineData("filter[caption]", "nin:first,last", "not(any(caption,'first','last'))")]
        [InlineData("filter[caption]", "isnull:", "equals(caption,null)")]
        [InlineData("filter[caption]", "isnotnull:", "not(equals(caption,null))")]
        [InlineData("filter[caption]", "unknown:some", "equals(caption,'unknown:some')")]
        [InlineData("filter[author.firstName]", "Jack", "equals(author.firstName,'Jack')")]
        [InlineData("filter", "expr:equals(caption,'some')", "equals(caption,'some')")]
        [InlineData("filter", "expr:equals(author,null)", "equals(author,null)")]
        [InlineData("filter", "expr:has(author.articles)", "has(author.articles)")]
        [InlineData("filter", "expr:equals(count(author.articles),'1')", "equals(count(author.articles),'1')")]
        public void Reader_Read_Succeeds(string parameterName, string parameterValue, string expressionExpected)
        {
            // Act
            _reader.Read(parameterName, parameterValue);

            var constraints = _reader.GetConstraints();

            // Assert
            var scope = constraints.Select(x => x.Scope).Single();
            scope.Should().BeNull();

            var value = constraints.Select(x => x.Expression).Single();
            value.ToString().Should().Be(expressionExpected);
        }
    }
}
