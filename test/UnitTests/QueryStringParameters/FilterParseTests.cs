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

namespace UnitTests.QueryStringParameters
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
        [InlineData("filter[articles.caption]", "equals(firstName,'some')", "Relationship 'caption' in 'articles.caption' does not exist on resource 'articles'.")]
        [InlineData("filter[articles.author]", "equals(firstName,'some')", "Relationship 'author' in 'articles.author' must be a to-many relationship on resource 'articles'.")]
        [InlineData("filter[articles.revisions.author]", "equals(firstName,'some')", "Relationship 'author' in 'articles.revisions.author' must be a to-many relationship on resource 'revisions'.")]
        [InlineData("filter[articles]", "equals(author,'some')", "Attribute 'author' does not exist on resource 'articles'.")]
        [InlineData("filter[articles]", "lessThan(author,null)", "Attribute 'author' does not exist on resource 'articles'.")]
        [InlineData("filter", " ", "Unexpected whitespace.")]
        [InlineData("filter", "some", "Filter function expected.")]
        [InlineData("filter", "equals", "( expected.")]
        [InlineData("filter", "equals'", "Unexpected ' outside text.")]
        [InlineData("filter", "equals(", "Count function or field name expected.")]
        [InlineData("filter", "equals('1'", "Count function or field name expected.")]
        [InlineData("filter", "equals(count(articles),", "Count function, value between quotes, null or field name expected.")]
        [InlineData("filter", "equals(title,')", "' expected.")]
        [InlineData("filter", "equals(title,null", ") expected.")]
        [InlineData("filter", "equals(null", "Field 'null' does not exist on resource 'blogs'.")]
        [InlineData("filter", "equals(title,(", "Count function, value between quotes, null or field name expected.")]
        [InlineData("filter", "equals(has(articles),'true')", "Field 'has' does not exist on resource 'blogs'.")]
        [InlineData("filter", "contains)", "( expected.")]
        [InlineData("filter", "contains(title,'a','b')", ") expected.")]
        [InlineData("filter", "contains(title,null)", "Value between quotes expected.")]
        [InlineData("filter[articles]", "contains(author,null)", "Attribute 'author' does not exist on resource 'articles'.")]
        [InlineData("filter", "any(null,'a','b')", "Attribute 'null' does not exist on resource 'blogs'.")]
        [InlineData("filter", "any('a','b','c')", "Field name expected.")]
        [InlineData("filter", "any(title,'b','c',)", "Value between quotes expected.")]
        [InlineData("filter", "any(title,'b')", ", expected.")]
        [InlineData("filter[articles]", "any(author,'a','b')", "Attribute 'author' does not exist on resource 'articles'.")]
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
            exception.Error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            exception.Error.Title.Should().Be("The specified filter is invalid.");
            exception.Error.Detail.Should().Be(errorMessage);
            exception.Error.Source.Parameter.Should().Be(parameterName);
        }

        [Theory]
        [InlineData("filter", "equals(title,'Brian O''Quote')", null, "equals(title,'Brian O''Quote')")]
        [InlineData("filter", "equals(title,'')", null, "equals(title,'')")]
        [InlineData("filter[articles]", "equals(caption,'this, that & more')", "articles", "equals(caption,'this, that & more')")]
        [InlineData("filter[owner.articles]", "equals(caption,'some')", "owner.articles", "equals(caption,'some')")]
        [InlineData("filter[articles.revisions]", "equals(publishTime,'2000-01-01')", "articles.revisions", "equals(publishTime,'2000-01-01')")]
        [InlineData("filter", "equals(count(articles),'1')", null, "equals(count(articles),'1')")]
        [InlineData("filter[articles]", "equals(caption,null)", "articles", "equals(caption,null)")]
        [InlineData("filter[articles]", "equals(author,null)", "articles", "equals(author,null)")]
        [InlineData("filter[articles]", "equals(author.firstName,author.lastName)", "articles", "equals(author.firstName,author.lastName)")]
        [InlineData("filter[articles.revisions]", "lessThan(publishTime,'2000-01-01')", "articles.revisions", "lessThan(publishTime,'2000-01-01')")]
        [InlineData("filter[articles.revisions]", "lessOrEqual(publishTime,'2000-01-01')", "articles.revisions", "lessOrEqual(publishTime,'2000-01-01')")]
        [InlineData("filter[articles.revisions]", "greaterThan(publishTime,'2000-01-01')", "articles.revisions", "greaterThan(publishTime,'2000-01-01')")]
        [InlineData("filter[articles.revisions]", "greaterOrEqual(publishTime,'2000-01-01')", "articles.revisions", "greaterOrEqual(publishTime,'2000-01-01')")]
        [InlineData("filter", "has(articles)", null, "has(articles)")]
        [InlineData("filter", "contains(title,'this')", null, "contains(title,'this')")]
        [InlineData("filter", "startsWith(title,'this')", null, "startsWith(title,'this')")]
        [InlineData("filter", "endsWith(title,'this')", null, "endsWith(title,'this')")]
        [InlineData("filter", "any(title,'this','that','there')", null, "any(title,'this','that','there')")]
        [InlineData("filter", "and(contains(title,'sales'),contains(title,'marketing'),contains(title,'advertising'))", null, "and(contains(title,'sales'),contains(title,'marketing'),contains(title,'advertising'))")]
        [InlineData("filter[articles]", "or(and(not(equals(author.firstName,null)),not(equals(author.lastName,null))),not(has(revisions)))", "articles", "or(and(not(equals(author.firstName,null)),not(equals(author.lastName,null))),not(has(revisions)))")]
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
