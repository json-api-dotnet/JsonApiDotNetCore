using System.ComponentModel.Design;
using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.QueryStringParameters;

public sealed class LegacyFilterParseTests : BaseParseTests
{
    private readonly FilterQueryStringParameterReader _reader;

    public LegacyFilterParseTests()
    {
        Options.EnableLegacyFilterNotation = true;

        Request.PrimaryResourceType = ResourceGraph.GetResourceType<BlogPost>();

        using var serviceProvider = new ServiceContainer();
        var resourceFactory = new ResourceFactory(serviceProvider);
        var scopeParser = new QueryStringParameterScopeParser();
        var valueParser = new FilterParser(resourceFactory);
        _reader = new FilterQueryStringParameterReader(scopeParser, valueParser, Request, ResourceGraph, Options);
    }

    [Theory]
    [InlineData("filter", "Field name between brackets expected in filter parameter name.")]
    [InlineData("filter[", "Field name between brackets expected in filter parameter name.")]
    [InlineData("filter[]", "Field name between brackets expected in filter parameter name.")]
    [InlineData("filter[some]", "Field 'some' does not exist on resource type 'blogPosts'.")]
    [InlineData("filter[author.posts]",
        "Field chain on resource type 'blogPosts' failed to match the pattern: zero or more to-one relationships or compound non-collection attributes, followed by a to-one relationship or an attribute. " +
        "To-one relationship or attribute on resource type 'webAccounts' expected.")]
    [InlineData("filter[some.id]", "Field 'some' does not exist on resource type 'blogPosts'.")]
    public void Reader_Read_ParameterName_Fails(string parameterName, string errorMessage)
    {
        // Act
        Action action = () => _reader.Read(parameterName, " ");

        // Assert
        InvalidQueryStringParameterException exception = action.Should().ThrowExactly<InvalidQueryStringParameterException>().And;

        exception.ParameterName.Should().Be(parameterName);
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified filter is invalid.");
        error.Detail.Should().Be(errorMessage);
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be(parameterName);
    }

    [Theory]
    [InlineData("filter[author]", "some",
        "Filtering on relationship 'author' is only supported using 'isnull:' or 'isnotnull:'. To filter by related attributes, use 'filter[author.id]=123' or use expression syntax.")]
    [InlineData("filter[author]", "eq:some",
        "Filtering on relationship 'author' is only supported using 'isnull:' or 'isnotnull:'. To filter by related attributes, use 'filter[author.id]=123' or use expression syntax.")]
    [InlineData("filter", "expr:equals(some,'other')", "Field 'some' does not exist on resource type 'blogPosts'.")]
    [InlineData("filter", "expr:equals(author,'Joe')", "To-one relationship name or null expected.")]
    [InlineData("filter", "expr:has(author)",
        "Field chain on resource type 'blogPosts' failed to match the pattern: zero or more to-one relationships or compound non-collection attributes, followed by a to-many relationship or a collection attribute. " +
        "Relationship or scalar collection attribute or compound attribute on resource type 'webAccounts' expected.")]
    [InlineData("filter", "expr:equals(count(author),'1')",
        "Field chain on resource type 'blogPosts' failed to match the pattern: zero or more to-one relationships or compound non-collection attributes, followed by a to-many relationship or a collection attribute. " +
        "Relationship or scalar collection attribute or compound attribute on resource type 'webAccounts' expected.")]
    public void Reader_Read_ParameterValue_Fails(string parameterName, string parameterValue, string errorMessage)
    {
        // Act
        Action action = () => _reader.Read(parameterName, parameterValue);

        // Assert
        InvalidQueryStringParameterException exception = action.Should().ThrowExactly<InvalidQueryStringParameterException>().And;

        exception.ParameterName.Should().Be(parameterName);
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified filter is invalid.");
        error.Detail.Should().Be(errorMessage);
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be(parameterName);
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

        // Assert
        RelationshipChainExpressionInScope[] constraints = _reader.GetConstraints().Cast<RelationshipChainExpressionInScope>().ToArray();
        ResourceFieldChainExpression? scope = constraints.Select(expressionInScope => expressionInScope.Scope).Single();
        scope.Should().BeNull();

        QueryExpression value = constraints.Select(expressionInScope => expressionInScope.Expression).Single();
        value.ToString().Should().Be(expressionExpected);
    }

    [Theory]
    // Through compounds and to-one relationships
    [InlineData("appointments", "filter[location.address.city]", "x", "equals(location.address.city,'x')")]
    [InlineData("appointments", "filter[location.address.city]", "like:x", "contains(location.address.city,'x')")]
    [InlineData("appointments", "filter[cost.amount]", "gt:5", "greaterThan(cost.amount,'5')")]
    [InlineData("appointments", "filter[calendar.workweek.firstDay]", "Monday", "equals(calendar.workweek.firstDay,'Monday')")]
    [InlineData("calendars", "filter[workweek.officeHours.opensAt]", "ge:09:00:00", "greaterOrEqual(workweek.officeHours.opensAt,'09:00:00')")]
    // Null checks on compounds, collections and relationships
    [InlineData("appointments", "filter[keywords]", "isnull:", "equals(keywords,null)")]
    [InlineData("appointments", "filter[keywords]", "isnotnull:", "not(equals(keywords,null))")]
    [InlineData("appointments", "filter[attendees]", "isnull:", "equals(attendees,null)")]
    [InlineData("appointments", "filter[location]", "isnull:", "equals(location,null)")]
    [InlineData("appointments", "filter[location]", "isnotnull:", "not(equals(location,null))")]
    [InlineData("appointments", "filter[cost.amount]", "isnull:", "equals(cost.amount,null)")]
    [InlineData("appointments", "filter[calendar]", "isnull:", "equals(calendar,null)")]
    [InlineData("appointments", "filter[calendar.workweek]", "isnull:", "equals(calendar.workweek,null)")]
    [InlineData("appointments", "filter[calendar.workweek.workDays]", "isnull:", "equals(calendar.workweek.workDays,null)")]
    [InlineData("calendars", "filter[holidays]", "isnull:", "equals(holidays,null)")]
    [InlineData("men", "filter[wife]", "isnotnull:", "not(equals(wife,null))")]
    // Expression syntax
    [InlineData("appointments", "filter", "expr:equals(location.address.city,'x')", "equals(location.address.city,'x')")]
    [InlineData("appointments", "filter", "expr:has(attendees,equals(name,'x'))", "has(attendees,equals(name,'x'))")]
    [InlineData("appointments", "filter", "expr:equals(keywords,null)", "equals(keywords,null)")]
    public void Reader_Read_Succeeds_On_Compound_Types(string resourceTypeName, string parameterName, string parameterValue, string expressionExpected)
    {
        // Arrange
        FilterQueryStringParameterReader reader = CreateReader(resourceTypeName);

        // Act
        reader.Read(parameterName, parameterValue);

        // Assert
        RelationshipChainExpressionInScope[] constraints = reader.GetConstraints().Cast<RelationshipChainExpressionInScope>().ToArray();
        ResourceFieldChainExpression? scope = constraints.Select(expressionInScope => expressionInScope.Scope).Single();
        scope.Should().BeNull();

        QueryExpression value = constraints.Select(expressionInScope => expressionInScope.Expression).Single();
        value.ToString().Should().Be(expressionExpected);
    }

    [Theory]
    // Only null may be compared against compounds and collections
    [InlineData("appointments", "filter[keywords]", "x", "null expected.")]
    [InlineData("appointments", "filter[attendees]", "x", "null expected.")]
    [InlineData("appointments", "filter[location]", "x", "null expected.")]
    [InlineData("appointments", "filter[calendar.workweek]", "x", "null expected.")]
    // Collections cannot be navigated into
    [InlineData("appointments", "filter[attendees.name]", "x",
        "Field chain on resource type 'appointments' failed to match the pattern: zero or more to-one relationships or compound non-collection attributes, followed by a to-one relationship or an attribute. " +
        "End of field chain expected.")]
    [InlineData("appointments", "filter[reminders]", "x",
        "Field chain on resource type 'appointments' failed to match the pattern: zero or more to-one relationships or compound non-collection attributes, followed by a to-one relationship or an attribute. " +
        "To-one relationship or attribute on resource type 'appointments' expected.")]
    [InlineData("appointments", "filter[reminders]", "isnull:",
        "Field chain on resource type 'appointments' failed to match the pattern: zero or more to-one relationships or compound non-collection attributes, followed by a to-one relationship or an attribute. " +
        "To-one relationship or attribute on resource type 'appointments' expected.")]
    // Unknown fields
    [InlineData("appointments", "filter[unknown]", "x", "Field 'unknown' does not exist on resource type 'appointments'.")]
    [InlineData("appointments", "filter[location.unknown]", "x", "Field 'unknown' does not exist on compound type 'AppointmentLocation'.")]
    // Relationships are pre-validated
    [InlineData("appointments", "filter[calendar]", "x",
        "Filtering on relationship 'calendar' is only supported using 'isnull:' or 'isnotnull:'. To filter by related attributes, use 'filter[calendar.id]=123' or use expression syntax.")]
    [InlineData("men", "filter[wife]", "x",
        "Filtering on relationship 'wife' is only supported using 'isnull:' or 'isnotnull:'. To filter by related attributes, use 'filter[wife.id]=123' or use expression syntax.")]
    [InlineData("humans", "filter[wife]", "x", "Field 'wife' does not exist on resource type 'humans'.")]
    // Expression values on a to-one relationship or compound chain are not a comparison, so the scope must be a to-many relationship
    [InlineData("appointments", "filter[calendar]", "expr:equals(timeZone,'UTC')", "To-many relationship 'calendar' does not exist on resource type 'appointments'.")]
    [InlineData("appointments", "filter[calendar.workweek]", "expr:equals(firstDay,'Monday')",
        "To-many relationship 'workweek' does not exist on resource type 'calendars'.")]
    [InlineData("men", "filter[wife]", "expr:equals(maidenName,'x')", "To-many relationship 'wife' does not exist on resource type 'men'.")]
    public void Reader_Read_ParameterValue_Fails_On_Compound_Types(string resourceTypeName, string parameterName, string parameterValue, string errorMessage)
    {
        // Arrange
        FilterQueryStringParameterReader reader = CreateReader(resourceTypeName);

        // Act
        Action action = () => reader.Read(parameterName, parameterValue);

        // Assert
        InvalidQueryStringParameterException exception = action.Should().ThrowExactly<InvalidQueryStringParameterException>().And;

        exception.ParameterName.Should().Be(parameterName);
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified filter is invalid.");
        error.Detail.Should().Be(errorMessage);
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be(parameterName);
    }

    private FilterQueryStringParameterReader CreateReader(string resourceTypeName)
    {
        Request.PrimaryResourceType = ResourceGraph.GetResourceType(resourceTypeName);

        using var serviceProvider = new ServiceContainer();
        var resourceFactory = new ResourceFactory(serviceProvider);
        var scopeParser = new QueryStringParameterScopeParser();
        var valueParser = new FilterParser(resourceFactory);
        return new FilterQueryStringParameterReader(scopeParser, valueParser, Request, ResourceGraph, Options);
    }
}
