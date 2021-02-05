using System;
using System.ComponentModel.Design;
using System.Linq;
using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.QueryStrings.Internal;
using JsonApiDotNetCore.Resources;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.UnitTests.QueryStringParameters
{
    public sealed class FilterParseTests : BaseParseTests
    {
        private readonly FilterQueryStringParameterReader _reader;

        public FilterParseTests()
        {
            Options.EnableLegacyFilterNotation = false;

            var resourceFactory = new ResourceFactory(new ServiceContainer());
            _reader = new FilterQueryStringParameterReader(Request, ResourceGraph, resourceFactory, Options);
        }

        [Theory]
        [InlineData("filter", true)]
        [InlineData("filter[title]", true)]
        [InlineData("filters", false)]
        [InlineData("filter[", false)]
        [InlineData("filter]", false)]
        public void Reader_Supports_Parameter_Name(string parameterName, bool expectCanParse)
        {
            // Act
            var canParse = _reader.CanRead(parameterName);

            // Assert
            canParse.Should().Be(expectCanParse);
        }

        [Theory]
        [InlineData(StandardQueryStringParameters.Filter, false)]
        [InlineData(StandardQueryStringParameters.All, false)]
        [InlineData(StandardQueryStringParameters.None, true)]
        [InlineData(StandardQueryStringParameters.Page, true)]
        public void Reader_Is_Enabled(StandardQueryStringParameters parametersDisabled, bool expectIsEnabled)
        {
            // Act
            var isEnabled = _reader.IsEnabled(new DisableQueryStringAttribute(parametersDisabled));

            // Assert
            isEnabled.Should().Be(expectIsEnabled);
        }

        [Theory]
        [InlineData("filter[", "equals(caption,'some')", "Field name expected.")]
        [InlineData("filter[caption]", "equals(url,'some')", "Relationship 'caption' does not exist on resource 'blogs'.")]
        [InlineData("filter[posts.caption]", "equals(firstName,'some')", "Relationship 'caption' in 'posts.caption' does not exist on resource 'blogPosts'.")]
        [InlineData("filter[posts.author]", "equals(firstName,'some')", "Relationship 'author' in 'posts.author' must be a to-many relationship on resource 'blogPosts'.")]
        [InlineData("filter[posts.comments.author]", "equals(firstName,'some')", "Relationship 'author' in 'posts.comments.author' must be a to-many relationship on resource 'comments'.")]
        [InlineData("filter[posts]", "equals(author,'some')", "Attribute 'author' does not exist on resource 'blogPosts'.")]
        [InlineData("filter[posts]", "lessThan(author,null)", "Attribute 'author' does not exist on resource 'blogPosts'.")]
        [InlineData("filter", " ", "Unexpected whitespace.")]
        [InlineData("filter", "some", "Filter function expected.")]
        [InlineData("filter", "equals", "( expected.")]
        [InlineData("filter", "equals'", "Unexpected ' outside text.")]
        [InlineData("filter", "equals(", "Count function or field name expected.")]
        [InlineData("filter", "equals('1'", "Count function or field name expected.")]
        [InlineData("filter", "equals(count(posts),", "Count function, value between quotes, null or field name expected.")]
        [InlineData("filter", "equals(title,')", "' expected.")]
        [InlineData("filter", "equals(title,null", ") expected.")]
        [InlineData("filter", "equals(null", "Field 'null' does not exist on resource 'blogs'.")]
        [InlineData("filter", "equals(title,(", "Count function, value between quotes, null or field name expected.")]
        [InlineData("filter", "equals(has(posts),'true')", "Field 'has' does not exist on resource 'blogs'.")]
        [InlineData("filter", "contains)", "( expected.")]
        [InlineData("filter", "contains(title,'a','b')", ") expected.")]
        [InlineData("filter", "contains(title,null)", "Value between quotes expected.")]
        [InlineData("filter[posts]", "contains(author,null)", "Attribute 'author' does not exist on resource 'blogPosts'.")]
        [InlineData("filter", "any(null,'a','b')", "Attribute 'null' does not exist on resource 'blogs'.")]
        [InlineData("filter", "any('a','b','c')", "Field name expected.")]
        [InlineData("filter", "any(title,'b','c',)", "Value between quotes expected.")]
        [InlineData("filter", "any(title,'b')", ", expected.")]
        [InlineData("filter[posts]", "any(author,'a','b')", "Attribute 'author' does not exist on resource 'blogPosts'.")]
        [InlineData("filter", "and(", "Filter function expected.")]
        [InlineData("filter", "or(equals(title,'some'),equals(title,'other')", ") expected.")]
        [InlineData("filter", "or(equals(title,'some'),equals(title,'other')))", "End of expression expected.")]
        [InlineData("filter", "and(equals(title,'some')", ", expected.")]
        [InlineData("filter", "and(null", "Filter function expected.")]
        [InlineData("filter", "expr:equals(caption,'some')", "Filter function expected.")]
        [InlineData("filter", "expr:Equals(caption,'some')", "Filter function expected.")]
        public void Reader_Read_Fails(string parameterName, string parameterValue, string errorMessage)
        {
            // Act
            Action action = () => _reader.Read(parameterName, parameterValue);

            // Assert
            var exception = action.Should().ThrowExactly<InvalidQueryStringParameterException>().And;

            exception.QueryParameterName.Should().Be(parameterName);
            exception.Errors.Should().HaveCount(1);
            exception.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            exception.Errors[0].Title.Should().Be("The specified filter is invalid.");
            exception.Errors[0].Detail.Should().Be(errorMessage);
            exception.Errors[0].Source.Parameter.Should().Be(parameterName);
        }

        [Theory]
        [InlineData("filter", "equals(title,'Brian O''Quote')", null, "equals(title,'Brian O''Quote')")]
        [InlineData("filter", "equals(title,'')", null, "equals(title,'')")]
        [InlineData("filter[posts]", "equals(caption,'this, that & more')", "posts", "equals(caption,'this, that & more')")]
        [InlineData("filter[owner.posts]", "equals(caption,'some')", "owner.posts", "equals(caption,'some')")]
        [InlineData("filter[posts.comments]", "equals(createdAt,'2000-01-01')", "posts.comments", "equals(createdAt,'2000-01-01')")]
        [InlineData("filter", "equals(count(posts),'1')", null, "equals(count(posts),'1')")]
        [InlineData("filter[posts]", "equals(caption,null)", "posts", "equals(caption,null)")]
        [InlineData("filter[posts]", "equals(author,null)", "posts", "equals(author,null)")]
        [InlineData("filter[posts]", "equals(author.userName,author.displayName)", "posts", "equals(author.userName,author.displayName)")]
        [InlineData("filter[posts.comments]", "lessThan(createdAt,'2000-01-01')", "posts.comments", "lessThan(createdAt,'2000-01-01')")]
        [InlineData("filter[posts.comments]", "lessOrEqual(createdAt,'2000-01-01')", "posts.comments", "lessOrEqual(createdAt,'2000-01-01')")]
        [InlineData("filter[posts.comments]", "greaterThan(createdAt,'2000-01-01')", "posts.comments", "greaterThan(createdAt,'2000-01-01')")]
        [InlineData("filter[posts.comments]", "greaterOrEqual(createdAt,'2000-01-01')", "posts.comments", "greaterOrEqual(createdAt,'2000-01-01')")]
        [InlineData("filter", "has(posts)", null, "has(posts)")]
        [InlineData("filter", "contains(title,'this')", null, "contains(title,'this')")]
        [InlineData("filter", "startsWith(title,'this')", null, "startsWith(title,'this')")]
        [InlineData("filter", "endsWith(title,'this')", null, "endsWith(title,'this')")]
        [InlineData("filter", "any(title,'this','that','there')", null, "any(title,'this','that','there')")]
        [InlineData("filter", "and(contains(title,'sales'),contains(title,'marketing'),contains(title,'advertising'))", null, "and(contains(title,'sales'),contains(title,'marketing'),contains(title,'advertising'))")]
        [InlineData("filter[posts]", "or(and(not(equals(author.userName,null)),not(equals(author.displayName,null))),not(has(comments)))", "posts", "or(and(not(equals(author.userName,null)),not(equals(author.displayName,null))),not(has(comments)))")]
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
