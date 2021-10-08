using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.QueryStrings.Internal;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.QueryStringParameters
{
    public sealed class LegacyFilterParseTests : BaseParseTests
    {
        private readonly FilterQueryStringParameterReader _reader;

        public LegacyFilterParseTests()
        {
            Options.EnableLegacyFilterNotation = true;

            Request.PrimaryResourceType = ResourceGraph.GetResourceType<BlogPost>();

            var resourceFactory = new ResourceFactory(new ServiceContainer());
            _reader = new FilterQueryStringParameterReader(Request, ResourceGraph, resourceFactory, Options);
        }

        [Theory]
        [InlineData("filter", "some", "Expected field name between brackets in filter parameter name.")]
        [InlineData("filter[", "some", "Expected field name between brackets in filter parameter name.")]
        [InlineData("filter[]", "some", "Expected field name between brackets in filter parameter name.")]
        [InlineData("filter[.]", "some", "Relationship '' in '.' does not exist on resource type 'blogPosts'.")]
        [InlineData("filter[some]", "other", "Field 'some' does not exist on resource type 'blogPosts'.")]
        [InlineData("filter[author]", "some", "Attribute 'author' does not exist on resource type 'blogPosts'.")]
        [InlineData("filter[author.posts]", "some",
            "Field 'posts' in 'author.posts' must be an attribute or a to-one relationship on resource type 'webAccounts'.")]
        [InlineData("filter[unknown.id]", "some", "Relationship 'unknown' in 'unknown.id' does not exist on resource type 'blogPosts'.")]
        [InlineData("filter", "expr:equals(some,'other')", "Field 'some' does not exist on resource type 'blogPosts'.")]
        [InlineData("filter", "expr:equals(author,'Joe')", "Attribute 'author' does not exist on resource type 'blogPosts'.")]
        [InlineData("filter", "expr:has(author)", "Relationship 'author' must be a to-many relationship on resource type 'blogPosts'.")]
        [InlineData("filter", "expr:equals(count(author),'1')", "Relationship 'author' must be a to-many relationship on resource type 'blogPosts'.")]
        public void Reader_Read_Fails(string parameterName, string parameterValue, string errorMessage)
        {
            // Act
            Action action = () => _reader.Read(parameterName, parameterValue);

            // Assert
            InvalidQueryStringParameterException exception = action.Should().ThrowExactly<InvalidQueryStringParameterException>().And;

            exception.QueryParameterName.Should().Be(parameterName);
            exception.Errors.Should().HaveCount(1);
            exception.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            exception.Errors[0].Title.Should().Be("The specified filter is invalid.");
            exception.Errors[0].Detail.Should().Be(errorMessage);
            exception.Errors[0].Source.Parameter.Should().Be(parameterName);
        }

        [Theory]
        [InlineData("filter[caption]", "Brian O'Quote", "equals(caption,'Brian O''Quote')")]
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
        [InlineData("filter[caption]", " ", "equals(caption,' ')")]
        [InlineData("filter[author.userName]", "Jack", "equals(author.userName,'Jack')")]
        [InlineData("filter", "expr:equals(caption,'some')", "equals(caption,'some')")]
        [InlineData("filter", "expr:equals(author,null)", "equals(author,null)")]
        [InlineData("filter", "expr:has(author.posts)", "has(author.posts)")]
        [InlineData("filter", "expr:equals(count(author.posts),'1')", "equals(count(author.posts),'1')")]
        [InlineData("filter[caption]", "using,comma", "or(equals(caption,'using'),equals(caption,'comma'))")]
        [InlineData("filter[caption]", "like:First,Second", "or(contains(caption,'First'),equals(caption,'Second'))")]
        [InlineData("filter[caption]", "like:First,like:Second", "or(contains(caption,'First'),contains(caption,'Second'))")]
        public void Reader_Read_Succeeds(string parameterName, string parameterValue, string expressionExpected)
        {
            // Act
            _reader.Read(parameterName, parameterValue);

            IReadOnlyCollection<ExpressionInScope> constraints = _reader.GetConstraints();

            // Assert
            ResourceFieldChainExpression scope = constraints.Select(expressionInScope => expressionInScope.Scope).Single();
            scope.Should().BeNull();

            QueryExpression value = constraints.Select(expressionInScope => expressionInScope.Expression).Single();
            value.ToString().Should().Be(expressionExpected);
        }
    }
}
