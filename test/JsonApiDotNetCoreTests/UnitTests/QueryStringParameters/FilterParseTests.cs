using System.ComponentModel.Design;
using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.QueryStringParameters;

public sealed class FilterParseTests : BaseParseTests
{
    private const string ScalarChain =
        "Field chain on resource type 'appointments' failed to match the pattern: zero or more to-one relationships or compound non-collection attributes, followed by a scalar non-collection attribute. ";

    private const string EqualityChain =
        "Field chain on resource type 'appointments' failed to match the pattern: zero or more to-one relationships or compound non-collection attributes, followed by a to-one relationship or an attribute. ";

    private const string CollectionChain =
        "Field chain on resource type 'appointments' failed to match the pattern: zero or more to-one relationships or compound non-collection attributes, followed by a to-many relationship or a collection attribute. ";

    private const string ToOneChain = "Field chain on resource type 'blogPosts' failed to match the pattern: one or more to-one relationships. ";

    private readonly FilterQueryStringParameterReader _reader;

    public FilterParseTests()
    {
        Options.EnableLegacyFilterNotation = false;

        using var serviceProvider = new ServiceContainer();
        var resourceFactory = new ResourceFactory(serviceProvider);
        var scopeParser = new QueryStringParameterScopeParser();
        var valueParser = new FilterParser(resourceFactory);
        _reader = new FilterQueryStringParameterReader(scopeParser, valueParser, Request, ResourceGraph, Options);
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
        bool canParse = _reader.CanRead(parameterName);

        // Assert
        canParse.Should().Be(expectCanParse);
    }

    [Theory]
    [InlineData(JsonApiQueryStringParameters.Filter, false)]
    [InlineData(JsonApiQueryStringParameters.All, false)]
    [InlineData(JsonApiQueryStringParameters.None, true)]
    [InlineData(JsonApiQueryStringParameters.Page, true)]
    public void Reader_Is_Enabled(JsonApiQueryStringParameters parametersDisabled, bool expectIsEnabled)
    {
        // Act
        bool isEnabled = _reader.IsEnabled(new DisableQueryStringAttribute(parametersDisabled));

        // Assert
        isEnabled.Should().Be(expectIsEnabled);
    }

    [Theory]
    [InlineData("filter[^", "To-many relationship name expected.")]
    [InlineData("filter[^.", "To-many relationship name expected.")]
    [InlineData("filter[posts.^]", "To-many relationship name expected.")]
    [InlineData("filter[posts.author.^]", "To-many relationship name expected.")]
    [InlineData("filter[^unknown]", "To-many relationship 'unknown' does not exist on resource type 'blogs'.")]
    [InlineData("filter[^unknown.other]", "Relationship 'unknown' does not exist on resource type 'blogs'.")]
    [InlineData("filter[posts.^caption]", "To-many relationship 'caption' does not exist on resource type 'blogPosts'.")]
    [InlineData("filter[posts.^author]", "To-many relationship 'author' does not exist on resource type 'blogPosts'.")]
    [InlineData("filter[posts.comments.^unknown]", "To-many relationship 'unknown' does not exist on resource type 'comments'.")]
    [InlineData("filter[posts.comments.^text]", "To-many relationship 'text' does not exist on resource type 'comments'.")]
    [InlineData("filter[posts.comments.^parent]", "To-many relationship 'parent' does not exist on resource type 'comments'.")]
    [InlineData("filter[owner.person.^unknown]", "To-many relationship 'unknown' does not exist on resource type 'humans' or any of its derived types.")]
    [InlineData("filter[owner.person.^unknown.other]", "Relationship 'unknown' does not exist on resource type 'humans' or any of its derived types.")]
    [InlineData("filter[owner.person.^hasBeard]", "To-many relationship 'hasBeard' does not exist on resource type 'humans' or any of its derived types.")]
    [InlineData("filter[owner.person.^wife]", "To-many relationship 'wife' does not exist on resource type 'humans' or any of its derived types.")]
    public void Reader_Read_ParameterName_Fails(string parameterName, string errorMessage)
    {
        // Arrange
        var parameterNameSource = new MarkedText(parameterName, '^');

        // Act
        Action action = () => _reader.Read(parameterNameSource.Text, " ");

        // Assert
        InvalidQueryStringParameterException exception = action.Should().ThrowExactly<InvalidQueryStringParameterException>().And;

        exception.ParameterName.Should().Be(parameterNameSource.Text);
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified filter is invalid.");
        error.Detail.Should().Be($"{errorMessage} {parameterNameSource}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be(parameterNameSource.Text);
    }

    [Theory]
    [InlineData("filter[posts]", "equals(author,^'some')", "To-one relationship name or null expected.")]
    [InlineData("filter[posts]", "lessThan(^some,null)", "Field 'some' does not exist on resource type 'blogPosts'.")]
    [InlineData("filter[posts]", "lessThan(author^,null)",
        "Field chain on resource type 'blogPosts' failed to match the pattern: zero or more to-one relationships or compound non-collection attributes, followed by a scalar non-collection attribute. " +
        "To-one relationship or non-collection attribute on resource type 'webAccounts' expected.")]
    [InlineData("filter", "^ ", "Unexpected whitespace.")]
    [InlineData("filter", "contains(owner.displayName^ ,)", "Unexpected whitespace.")]
    [InlineData("filter", "contains(owner.displayName,^ )", "Unexpected whitespace.")]
    [InlineData("filter", "^some", "Filter function expected.")]
    [InlineData("filter", "equals^", "( expected.")]
    [InlineData("filter", "equals^'", "Unexpected ' outside quoted text.")]
    [InlineData("filter", "equals(^", "Function or field name expected.")]
    [InlineData("filter", "equals(^'1'", "Function or field name expected.")]
    [InlineData("filter", "equals(count(posts),^", "Function, field name, value between quotes or null expected.")]
    [InlineData("filter", "greaterThan(count(posts),^null)", "Function, field name or value between quotes expected.")]
    [InlineData("filter", "greaterThan(showAdvertisements,^null)", "Function, field name or value between quotes expected.")]
    [InlineData("filter", "equals(owner.^.displayName,'')", "Field name expected.")]
    [InlineData("filter", "equals(owner.displayName.^,'')", "Field name expected.")]
    [InlineData("filter", "equals(title,'^)", "Closing ' expected.")]
    [InlineData("filter", "equals(title,^(", "Function, field name, value between quotes or null expected.")]
    [InlineData("filter", "equals(description,null^", ") expected.")]
    [InlineData("filter", "equals(description,^(", "Function, field name, value between quotes or null expected.")]
    [InlineData("filter", "equals(shareCount,null^", ") expected.")]
    [InlineData("filter", "equals(shareCount,^(", "Function, field name, value between quotes or null expected.")]
    [InlineData("filter", "has(tags,equals(^unknown,'work'))", "Function or _ expected.")]
    [InlineData("filter", "has(tags,contains(^unknown,'work'))", "Function or _ expected.")]
    [InlineData("filter", "has(tags,any(^unknown))", "Function or _ expected.")]
    [InlineData("filter", "has(tags,^has(tags,equals(_,'w')))", "Function 'has' cannot be used on scalar collection elements.")]
    [InlineData("filter", "has(tags,equals(^count(tags),'1'))", "Function 'count' cannot be used on scalar collection elements.")]
    [InlineData("filter", "has(tags,^isType(,some))", "Function 'isType' can only be used on resource types.")]
    [InlineData("filter", "has(tags,equals(_,^_))", "Function, value between quotes or null expected.")]
    [InlineData("filter", "has(tags,startsWith(_,^_))", "Value between quotes expected.")]
    [InlineData("filter", "equals(title,^_)", "Function, field name, value between quotes or null expected.")]
    [InlineData("filter", "equals(owner,^'some')", "To-one relationship name or null expected.")]
    [InlineData("filter", "equals(^posts,null)",
        "Field chain on resource type 'blogs' failed to match the pattern: zero or more to-one relationships or compound non-collection attributes, followed by a to-one relationship or an attribute. " +
        "To-one relationship or attribute on resource type 'blogs' expected.")]
    [InlineData("filter", "equals(^null", "Function or field name expected.")]
    [InlineData("filter", "has(posts,^", "Filter function expected.")]
    [InlineData("filter", "contains^)", "( expected.")]
    [InlineData("filter", "contains(title,'a'^,'b')", ") expected.")]
    [InlineData("filter", "contains(^equals(title,'x'),'a')", "Function returns type 'Boolean' while type 'String' was expected.")]
    [InlineData("filter", "contains(title,^null)", "Value between quotes expected.")]
    [InlineData("filter[posts]", "contains(author^,null)",
        "Field chain on resource type 'blogPosts' failed to match the pattern: zero or more to-one relationships or compound non-collection attributes, followed by a scalar non-collection attribute. " +
        "To-one relationship or non-collection attribute on resource type 'webAccounts' expected.")]
    [InlineData("filter", "any(^null,'a','b')", "Function or field name expected.")]
    [InlineData("filter", "any(^'a','b','c')", "Function or field name expected.")]
    [InlineData("filter", "any(title,'b','c',^)", "Value between quotes expected.")]
    [InlineData("filter", "any(equals(title,'x'),^'b')", "Failed to convert 'b' to type 'Boolean'.")]
    [InlineData("filter", "any(title^)", ", expected.")]
    [InlineData("filter[posts]", "any(author^,'a','b')",
        "Field chain on resource type 'blogPosts' failed to match the pattern: zero or more to-one relationships or compound non-collection attributes, followed by a scalar non-collection attribute. " +
        "To-one relationship or non-collection attribute on resource type 'webAccounts' expected.")]
    [InlineData("filter", "and(^", "Filter function expected.")]
    [InlineData("filter", "or(equals(title,'some'),equals(title,'other')^", ") expected.")]
    [InlineData("filter", "or(equals(title,'some'),equals(title,'other'))^)", "End of expression expected.")]
    [InlineData("filter", "and(equals(title,'some')^", ", expected.")]
    [InlineData("filter", "and(^null", "Filter function expected.")]
    [InlineData("filter", "^expr:equals(caption,'some')", "Filter function expected.")]
    [InlineData("filter", "^expr:Equals(caption,'some')", "Filter function expected.")]
    [InlineData("filter", "isType(^", "To-one relationship name or , expected.")]
    [InlineData("filter", "isType(,^", "Resource type expected.")]
    [InlineData("filter[posts.contributors]", "isType(,^some)", "Resource type 'some' does not exist or does not derive from 'humans'.")]
    [InlineData("filter[posts.contributors]", "isType(,^humans)", "Resource type 'humans' does not exist or does not derive from 'humans'.")]
    [InlineData("filter[posts.contributors]", "isType(^some,men)", "Field 'some' does not exist on resource type 'humans'.")]
    [InlineData("filter[posts.contributors]", "isType(father.^some,women)", "Field 'some' does not exist on resource type 'men'.")]
    [InlineData("filter[posts.contributors]", "isType(^children,men)",
        "Field chain on resource type 'humans' failed to match the pattern: one or more to-one relationships. " +
        "To-one relationship on resource type 'humans' expected.")]
    [InlineData("filter[posts.contributors]", "isType(mother.^children,men)",
        "Field chain on resource type 'humans' failed to match the pattern: one or more to-one relationships. " +
        "End of field chain or to-one relationship on resource type 'women' expected.")]
    public void Reader_Read_ParameterValue_Fails(string parameterName, string parameterValue, string errorMessage)
    {
        // Arrange
        var parameterValueSource = new MarkedText(parameterValue, '^');

        // Act
        Action action = () => _reader.Read(parameterName, parameterValueSource.Text);

        // Assert
        InvalidQueryStringParameterException exception = action.Should().ThrowExactly<InvalidQueryStringParameterException>().And;

        exception.ParameterName.Should().Be(parameterName);
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified filter is invalid.");
        error.Detail.Should().Be($"{errorMessage} {parameterValueSource}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be(parameterName);
    }

    [Theory]
    [InlineData("filter", "equals(title,'Brian O''Quote')", null)]
    [InlineData("filter", "equals(title,'!@#$%^&*()-_=+\"''[]{}<>()/|\\:;.,`~')", null)]
    [InlineData("filter", "equals(title,'')", null)]
    [InlineData("filter", "startsWith(owner.displayName,'GivenName ')", null)]
    [InlineData("filter", "endsWith(owner.displayName,' Surname')", null)]
    [InlineData("filter", "contains(owner.displayName,' ')", null)]
    [InlineData("filter[posts]", "equals(caption,'this, that & more')", "posts")]
    [InlineData("filter[owner.posts]", "equals(caption,'some')", "owner.posts")]
    [InlineData("filter[posts.comments]", "equals(createdAt,'2000-01-01')", "posts.comments")]
    [InlineData("filter[owner.person.wife.children]", "not(equals(mother,null))", "owner.person.wife.children")]
    [InlineData("filter", "equals(count(posts),'1')", null)]
    [InlineData("filter", "equals(count(posts),count(owner.posts))", null)]
    [InlineData("filter", "equals(count(posts),null)", null)]
    [InlineData("filter", "equals(showAdvertisements,null)", null)]
    [InlineData("filter", "equals(owner.preferences.useDarkTheme,null)", null)]
    [InlineData("filter", "equals(has(posts),'true')", null)]
    [InlineData("filter", "equals(description,null)", null)]
    [InlineData("filter", "equals(shareCount,null)", null)]
    [InlineData("filter", "equals(owner,null)", null)]
    [InlineData("filter", "equals(tags,null)", null)]
    [InlineData("filter", "not(equals(tags,null))", null)]
    [InlineData("filter", "has(tags)", null)]
    [InlineData("filter", "has(tags,equals(_,'work'))", null)]
    [InlineData("filter", "has(tags,startsWith(_,'w'))", null)]
    [InlineData("filter", "has(tags,contains(_,'o'))", null)]
    [InlineData("filter", "has(tags,endsWith(_,'k'))", null)]
    [InlineData("filter", "has(tags,any(_,'work'))", null)]
    [InlineData("filter", "has(tags,not(equals(_,'spam')))", null)]
    [InlineData("filter[posts]", "equals(caption,null)", "posts")]
    [InlineData("filter[posts]", "equals(author,null)", "posts")]
    [InlineData("filter[posts]", "equals(author,reviewer)", "posts")]
    [InlineData("filter[posts]", "equals(author.userName,author.displayName)", "posts")]
    [InlineData("filter[posts.comments]", "lessThan(createdAt,'2000-01-01')", "posts.comments")]
    [InlineData("filter[posts.comments]", "lessOrEqual(createdAt,'2000-01-01')", "posts.comments")]
    [InlineData("filter[posts.comments]", "greaterThan(createdAt,'2000-01-01')", "posts.comments")]
    [InlineData("filter[posts.comments]", "greaterOrEqual(createdAt,'2000-01-01')", "posts.comments")]
    [InlineData("filter", "has(posts)", null)]
    [InlineData("filter", "has(posts,not(equals(url,null)))", null)]
    [InlineData("filter", "contains(title,'this')", null)]
    [InlineData("filter", "startsWith(title,'this')", null)]
    [InlineData("filter", "endsWith(title,'this')", null)]
    [InlineData("filter", "any(title,'this')", null)]
    [InlineData("filter", "any(title,'that','there','this')", null)]
    [InlineData("filter", "any(equals(title,'x'),'true')", null)]
    [InlineData("filter", "and(contains(title,'sales'),contains(title,'marketing'),contains(title,'advertising'))", null)]
    [InlineData("filter", "and(contains(title,'a'),contains(title,'b'),contains(title,'c'),contains(title,'d'))", null)]
    [InlineData("filter", "or(equals(title,'a'),equals(title,'b'),equals(title,'c'),equals(title,'d'),equals(title,'e'))", null)]
    [InlineData("filter", "and(equals(title,'a'),or(equals(title,'b'),equals(title,'c'),equals(title,'d'),equals(title,'e')),not(equals(title,'f')),has(posts))",
        null)]
    [InlineData("filter[posts]", "or(and(not(equals(author.userName,null)),not(equals(author.displayName,null))),not(has(comments,startsWith(text,'A'))))",
        "posts")]
    [InlineData("filter", "isType(owner.person,men)", null)]
    [InlineData("filter", "isType(owner.person,men,equals(hasBeard,'true'))", null)]
    [InlineData("filter[posts.contributors]", "isType(,women)", "posts.contributors")]
    [InlineData("filter[posts.contributors]", "isType(,women,equals(maidenName,'Austen'))", "posts.contributors")]
    public void Reader_Read_Succeeds(string parameterName, string parameterValue, string? scopeExpected)
    {
        // Act
        _reader.Read(parameterName, parameterValue);

        // Assert
        RelationshipChainExpressionInScope[] constraints = _reader.GetConstraints().Cast<RelationshipChainExpressionInScope>().ToArray();
        ResourceFieldChainExpression? scope = constraints.Select(expressionInScope => expressionInScope.Scope).Single();

        scope?.ToString().Should().Be(scopeExpected);

        QueryExpression value = constraints.Select(expressionInScope => expressionInScope.Expression).Single();
        value.ToString().Should().Be(parameterValue);
    }

    [Theory]
    // Nested attributes in compound types, through compounds and to-one relationships
    [InlineData("appointments", "filter", "equals(location.address.city,'x')", null)]
    [InlineData("appointments", "filter", "equals(location.address.city,null)", null)]
    [InlineData("appointments", "filter", "equals(location.name,location.room)", null)]
    [InlineData("appointments", "filter", "equals(location.name,calendar.timeZone)", null)]
    [InlineData("appointments", "filter", "equals(cost.amount,null)", null)]
    [InlineData("calendars", "filter", "equals(workweek.firstDay,'Monday')", null)]
    [InlineData("calendars", "filter", "equals(workweek.officeHours.opensAt,'09:00:00')", null)]
    [InlineData("calendars", "filter", "equals(workweek.secret,'x')", null)]
    [InlineData("calendars", "filter", "equals(mostRecentAppointment.location.address.city,'x')", null)]
    // Null checks on compound types, compound collections and scalar collections
    [InlineData("appointments", "filter", "equals(location,null)", null)]
    [InlineData("appointments", "filter", "equals(location.address,null)", null)]
    [InlineData("appointments", "filter", "equals(cost,null)", null)]
    [InlineData("appointments", "filter", "equals(calendar.workweek,null)", null)]
    [InlineData("appointments", "filter", "equals(attendees,null)", null)]
    [InlineData("appointments", "filter", "equals(keywords,null)", null)]
    [InlineData("appointments", "filter", "not(equals(keywords,null))", null)]
    [InlineData("calendars", "filter", "equals(workweek,null)", null)]
    [InlineData("calendars", "filter", "equals(workweek.officeHours,null)", null)]
    [InlineData("calendars", "filter", "equals(mostRecentAppointment.attendees,null)", null)]
    [InlineData("calendars", "filter", "equals(mostRecentAppointment,null)", null)]
    // Functions over nested paths
    [InlineData("appointments", "filter", "any(location.address.city,'a','b')", null)]
    [InlineData("appointments", "filter", "contains(location.address.city,'x')", null)]
    [InlineData("appointments", "filter", "startsWith(location.address.city,'x')", null)]
    [InlineData("appointments", "filter", "endsWith(location.address.city,'x')", null)]
    [InlineData("appointments", "filter", "and(equals(location.name,'x'),not(equals(cost.amount,'1')))", null)]
    // Count and has on collections
    [InlineData("appointments", "filter", "equals(count(keywords),'1')", null)]
    [InlineData("appointments", "filter", "equals(count(attendees),'1')", null)]
    [InlineData("appointments", "filter", "equals(count(reminders),'1')", null)]
    [InlineData("appointments", "filter", "equals(count(calendar.holidays),'1')", null)]
    [InlineData("calendars", "filter", "equals(count(holidays),'1')", null)]
    [InlineData("calendars", "filter", "equals(count(workweek.workDays),'1')", null)]
    [InlineData("calendars", "filter", "equals(count(mostRecentAppointment.attendees),'1')", null)]
    [InlineData("appointments", "filter", "has(keywords)", null)]
    [InlineData("appointments", "filter", "has(attendees)", null)]
    [InlineData("appointments", "filter", "has(reminders)", null)]
    [InlineData("appointments", "filter", "has(calendar.holidays)", null)]
    [InlineData("calendars", "filter", "has(workweek.workDays)", null)]
    [InlineData("calendars", "filter", "has(mostRecentAppointment.attendees)", null)]
    [InlineData("calendars", "filter", "has(mostRecentAppointment.keywords)", null)]
    // Predicates inside the scope of a compound collection
    [InlineData("appointments", "filter", "has(attendees,equals(name,'x'))", null)]
    [InlineData("appointments", "filter", "has(attendees,contains(name,'x'))", null)]
    [InlineData("appointments", "filter", "has(attendees,startsWith(contact.title,'x'))", null)]
    [InlineData("appointments", "filter", "has(attendees,any(name,'a','b'))", null)]
    [InlineData("appointments", "filter", "has(attendees,not(equals(name,'x')))", null)]
    [InlineData("appointments", "filter", "has(attendees,equals(response,'Accepted'))", null)]
    [InlineData("appointments", "filter", "has(attendees,equals(response,null))", null)]
    [InlineData("appointments", "filter", "has(attendees,equals(name,email))", null)]
    [InlineData("appointments", "filter", "has(attendees,and(equals(name,'a'),equals(email,null)))", null)]
    [InlineData("calendars", "filter", "has(holidays,equals(name,'x'))", null)]
    [InlineData("calendars", "filter", "has(holidays,equals(isPublic,'true'))", null)]
    [InlineData("calendars", "filter", "has(holidays,equals(date,'2020-01-01'))", null)]
    [InlineData("calendars", "filter", "has(holidays,equals(date,null))", null)]
    // Element scope on scalar collections
    [InlineData("appointments", "filter", "has(keywords,equals(_,'x'))", null)]
    [InlineData("appointments", "filter", "has(keywords,lessThan(_,'x'))", null)]
    [InlineData("appointments", "filter", "has(keywords,greaterThan(_,'x'))", null)]
    [InlineData("appointments", "filter", "has(keywords,lessOrEqual(_,'x'))", null)]
    [InlineData("appointments", "filter", "has(keywords,greaterOrEqual(_,'x'))", null)]
    [InlineData("appointments", "filter", "has(keywords,contains(_,'x'))", null)]
    [InlineData("appointments", "filter", "has(keywords,startsWith(_,'x'))", null)]
    [InlineData("appointments", "filter", "has(keywords,endsWith(_,'x'))", null)]
    [InlineData("appointments", "filter", "has(keywords,any(_,'x','y'))", null)]
    [InlineData("appointments", "filter", "has(keywords,equals(_,null))", null)]
    [InlineData("calendars", "filter", "has(workweek.workDays,equals(_,'Monday'))", null)]
    [InlineData("calendars", "filter", "has(workweek.workDays,equals(_,null))", null)]
    // To-one equality
    [InlineData("blogs", "filter", "has(posts,equals(author,reviewer))", null)]
    // Scopes
    [InlineData("calendars", "filter[appointments]", "equals(location.name,'x')", "appointments")]
    [InlineData("appointments", "filter[calendar.appointments]", "equals(location.name,'x')", "calendar.appointments")]
    public void Reader_Read_Succeeds_On_Compound_Types(string resourceTypeName, string parameterName, string parameterValue, string? scopeExpected)
    {
        // Arrange
        FilterQueryStringParameterReader reader = CreateReader(resourceTypeName);

        // Act
        reader.Read(parameterName, parameterValue);

        // Assert
        RelationshipChainExpressionInScope[] constraints = reader.GetConstraints().Cast<RelationshipChainExpressionInScope>().ToArray();
        ResourceFieldChainExpression? scope = constraints.Select(expressionInScope => expressionInScope.Scope).Single();

        scope?.ToString().Should().Be(scopeExpected);

        QueryExpression value = constraints.Select(expressionInScope => expressionInScope.Expression).Single();
        value.ToString().Should().Be(parameterValue);
    }

    [Theory]
    [InlineData("calendars", "filter[^workweek]", "To-many relationship 'workweek' does not exist on resource type 'calendars'.")]
    [InlineData("calendars", "filter[^holidays]", "To-many relationship 'holidays' does not exist on resource type 'calendars'.")]
    [InlineData("calendars", "filter[^workweek.workDays]", "Relationship 'workweek' does not exist on resource type 'calendars'.")]
    [InlineData("calendars", "filter[mostRecentAppointment.^attendees]", "To-many relationship 'attendees' does not exist on resource type 'appointments'.")]
    [InlineData("appointments", "filter[^location]", "To-many relationship 'location' does not exist on resource type 'appointments'.")]
    [InlineData("appointments", "filter[calendar.^workweek]", "To-many relationship 'workweek' does not exist on resource type 'calendars'.")]
    public void Reader_Read_ParameterName_Fails_On_Compound_Types(string resourceTypeName, string parameterName, string errorMessage)
    {
        // Arrange
        FilterQueryStringParameterReader reader = CreateReader(resourceTypeName);
        var parameterNameSource = new MarkedText(parameterName, '^');

        // Act
        Action action = () => reader.Read(parameterNameSource.Text, "equals(x,'1')");

        // Assert
        InvalidQueryStringParameterException exception = action.Should().ThrowExactly<InvalidQueryStringParameterException>().And;

        exception.ParameterName.Should().Be(parameterNameSource.Text);
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified filter is invalid.");
        error.Detail.Should().Be($"{errorMessage} {parameterNameSource}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be(parameterNameSource.Text);
    }

    [Theory]
    // Null is only allowed for equality
    [InlineData("appointments", "lessThan(cost.amount,^null)", "Function, field name or value between quotes expected.")]
    // Compound and collection attributes are rejected where a scalar is expected
    [InlineData("appointments", "greaterThan(location^,null)", ScalarChain + "Non-collection attribute on compound type 'AppointmentLocation' expected.")]
    [InlineData("appointments", "greaterThan(^keywords,null)", ScalarChain + "To-one relationship or non-collection attribute on resource type 'appointments' expected.")]
    [InlineData("appointments", "lessThan(^attendees,null)", ScalarChain + "To-one relationship or non-collection attribute on resource type 'appointments' expected.")]
    [InlineData("appointments", "contains(^keywords,'x')", ScalarChain + "To-one relationship or non-collection attribute on resource type 'appointments' expected.")]
    // Only null may be compared against compound and collection attributes
    [InlineData("appointments", "equals(location,^'x')", "null expected.")]
    [InlineData("appointments", "equals(attendees,^'x')", "null expected.")]
    [InlineData("appointments", "equals(keywords,^'x')", "null expected.")]
    [InlineData("appointments", "equals(cost,^'x')", "null expected.")]
    // Collection attributes cannot be navigated into
    [InlineData("appointments", "equals(attendees.^name,'1')", EqualityChain + "End of field chain expected.")]
    [InlineData("appointments", "equals(keywords.^x,'1')", EqualityChain + "End of field chain expected.")]
    // Count and has require a collection
    [InlineData("appointments", "greaterThan(count(location^),'0')",
        CollectionChain + "Scalar collection attribute or compound attribute on compound type 'AppointmentLocation' expected.")]
    [InlineData("appointments", "greaterThan(count(location.address.^city),'0')",
        CollectionChain + "Scalar collection attribute or compound attribute on compound type 'AppointmentAddress' expected.")]
    [InlineData("appointments", "has(location^)", CollectionChain + "Scalar collection attribute or compound attribute on compound type 'AppointmentLocation' expected.")]
    [InlineData("appointments", "has(cost.^amount)", CollectionChain + "Scalar collection attribute or compound attribute on compound type 'AppointmentCost' expected.")]
    // Scope of a compound collection
    [InlineData("appointments", "has(attendees,equals(^unknown,'x'))", "Field 'unknown' does not exist on compound type 'Attendee'.")]
    [InlineData("appointments", "has(attendees,has(^keywords))", "Field 'keywords' does not exist on compound type 'Attendee'.")]
    [InlineData("appointments", "has(attendees,^isType(x,y))", "Function 'isType' can only be used on resource types.")]
    [InlineData("appointments", "has(attendees,^count(name))", "Filter function expected.")]
    [InlineData("appointments", "has(attendees,equals(^_,'x'))", "Function or field name expected.")]
    [InlineData("appointments", "has(attendees,equals(response,^'Bogus'))", "Failed to convert 'Bogus' to type 'Nullable<AttendeeResponse>'.")]
    [InlineData("appointments", "has(attendees,startsWith(^_.name,'x'))", "Function or field name expected.")]
    [InlineData("appointments", "has(attendees,contains(^_,'x'))", "Function or field name expected.")]
    [InlineData("appointments", "has(attendees,endsWith(^null,'x'))", "Function or field name expected.")]
    [InlineData("appointments", "has(attendees,startsWith(^response,'x'))",
        "Attribute 'response' is of type 'Nullable<AttendeeResponse>' while type 'String' was expected.")]
    [InlineData("appointments", "has(attendees,contains(contact.^unknown,'x'))", "Field 'unknown' does not exist on compound type 'AttendeeContact'.")]
    // Element scope of a scalar collection
    [InlineData("appointments", "equals(^_,'x')", "Function or field name expected.")]
    [InlineData("appointments", "has(keywords,equals(^x,'1'))", "Function or _ expected.")]
    [InlineData("appointments", "has(keywords,^count(_))", "Filter function expected.")]
    [InlineData("appointments", "has(keywords,^has(_))", "Function 'has' cannot be used on scalar collection elements.")]
    [InlineData("appointments", "has(keywords,^isType(_,x))", "Function 'isType' can only be used on resource types.")]
    [InlineData("calendars", "has(workweek.workDays,contains(^_,'x'))", "Element is of type 'DayOfWeek' while type 'String' was expected.")]
    [InlineData("calendars", "has(workweek.workDays,equals(_,^'Bogus'))", "Failed to convert 'Bogus' to type 'DayOfWeek'.")]
    [InlineData("calendars", "has(holidays,equals(date,^'x'))", "Failed to convert 'x' to type 'DateOnly'.")]
    [InlineData("calendars", "equals(workweek.officeHours.opensAt,^'x')", "Failed to convert 'x' to type 'TimeOnly'.")]
    // Capabilities on nested attributes
    [InlineData("appointments", "equals(calendar.workweek.^internalNote,'x')", "Filtering on attribute 'internalNote' is not allowed.")]
    [InlineData("calendars", "equals(workweek.^internalNote,'x')", "Filtering on attribute 'internalNote' is not allowed.")]
    [InlineData("calendars", "equals(workweek.^internalNote,null)", "Filtering on attribute 'internalNote' is not allowed.")]
    [InlineData("calendars", "equals(^audit.createdBy,'x')", "Filtering on attribute 'audit' is not allowed.")]
    [InlineData("calendars", "equals(^audit,null)", "Filtering on attribute 'audit' is not allowed.")]
    [InlineData("calendars", "has(^appointments)", "Filtering on relationship 'appointments' is not allowed.")]
    // Position of the last field in a chain is reported for text matches on non-string attributes
    [InlineData("appointments", "contains(cost.^amount,'x')", "Attribute 'amount' is of type 'Decimal' while type 'String' was expected.")]
    [InlineData("calendars", "startsWith(workweek.officeHours.^opensAt,'x')", "Attribute 'opensAt' is of type 'TimeOnly' while type 'String' was expected.")]
    [InlineData("appointments", "endsWith(calendar.workweek.^firstDay,'x')", "Attribute 'firstDay' is of type 'DayOfWeek' while type 'String' was expected.")]
    // Element and function operands
    [InlineData("calendars", "has(workweek.workDays,equals(_,^_))", "Function, value between quotes or null expected.")]
    [InlineData("calendars", "has(workweek.workDays,lessThan(_,^_))", "Function or value between quotes expected.")]
    [InlineData("appointments", "has(keywords,equals(_,^_))", "Function, value between quotes or null expected.")]
    [InlineData("appointments", "has(keywords,any(_,^_))", "Value between quotes expected.")]
    // Equality between to-one relationships
    [InlineData("blogPosts", "equals(author,^caption)", ToOneChain + "To-one relationship on resource type 'blogPosts' expected.")]
    [InlineData("blogPosts", "equals(author,^comments)", ToOneChain + "To-one relationship on resource type 'blogPosts' expected.")]
    [InlineData("blogPosts", "equals(author,reviewer.^userName)", ToOneChain + "End of field chain or to-one relationship on resource type 'webAccounts' expected.")]
    public void Reader_Read_ParameterValue_Fails_On_Compound_Types(string resourceTypeName, string parameterValue, string errorMessage)
    {
        // Arrange
        FilterQueryStringParameterReader reader = CreateReader(resourceTypeName);
        var parameterValueSource = new MarkedText(parameterValue, '^');

        // Act
        Action action = () => reader.Read("filter", parameterValueSource.Text);

        // Assert
        InvalidQueryStringParameterException exception = action.Should().ThrowExactly<InvalidQueryStringParameterException>().And;

        exception.ParameterName.Should().Be("filter");
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified filter is invalid.");
        error.Detail.Should().Be($"{errorMessage} {parameterValueSource}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("filter");
    }

    [Fact]
    public void Reader_Read_Field_Name_Starting_With_Underscore_Is_Reported_As_Unknown_Field()
    {
        // Arrange
        FilterQueryStringParameterReader reader = CreateReader("appointments");
        var parameterValueSource = new MarkedText("equals(^_x,'1')", '^');

        // Act
        Action action = () => reader.Read("filter", parameterValueSource.Text);

        // Assert
        InvalidQueryStringParameterException exception = action.Should().ThrowExactly<InvalidQueryStringParameterException>().And;

        ErrorObject error = exception.Errors.Should().ContainSingle().Which;
        error.Detail.Should().Be($"Field '_x' does not exist on resource type 'appointments'. {parameterValueSource}");
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

    [Fact]
    public void Throws_When_ResourceType_Scope_Not_Disposed()
    {
        // Arrange
        using var serviceProvider = new ServiceContainer();
        var resourceFactory = new ResourceFactory(serviceProvider);
        var parser = new NotDisposingContainerScopeFilterParser(resourceFactory);

        // Act
        Action action = () => parser.Parse("equals(title,'some')", Request.PrimaryResourceType!);

        // Assert
        action.Should().ThrowExactly<InvalidOperationException>().WithMessage("There is still a resource type in scope after parsing has completed. " +
            "Verify that Dispose() is called on all return values of InScopeOfContainer().");
    }

    [Fact]
    public void Throws_When_ScalarCollection_Scope_Not_Disposed()
    {
        // Arrange
        using var serviceProvider = new ServiceContainer();
        var resourceFactory = new ResourceFactory(serviceProvider);
        var parser = new NotDisposingScalarCollectionScopeFilterParser(resourceFactory);

        // Act
        Action action = () => parser.Parse("equals(_,'some')", Request.PrimaryResourceType!);

        // Assert
        action.Should().ThrowExactly<InvalidOperationException>().WithMessage(
            "There is still a scalar collection element type in scope after parsing has completed. " +
            "Verify that Dispose() is called on all return values of InScopeOfScalarCollection().");
    }

    [Fact]
    public void Throws_When_No_Filter_Scope_Is_Active()
    {
        // Arrange
        using var serviceProvider = new ServiceContainer();
        var resourceFactory = new ResourceFactory(serviceProvider);
        var parser = new ScopeAccessingFilterParser(resourceFactory);

        // Act
        Action action = () => parser.Parse("equals(title,'some')", Request.PrimaryResourceType!);

        // Assert
        action.Should().ThrowExactly<InvalidOperationException>().WithMessage("No filter scope is currently active. Call Parse() first.");
    }

    private sealed class NotDisposingContainerScopeFilterParser(IResourceFactory resourceFactory)
        : FilterParser(resourceFactory)
    {
        protected override FilterExpression ParseFilter()
        {
            // Forgot to dispose the return value.
            _ = InScopeOfContainer(((AttributeContainerFilterScope)CurrentScope).Container);

            return base.ParseFilter();
        }
    }

    private sealed class NotDisposingScalarCollectionScopeFilterParser(IResourceFactory resourceFactory)
        : FilterParser(resourceFactory)
    {
        protected override FilterExpression ParseFilter()
        {
            // Forgot to dispose the return value.
            _ = InScopeOfScalarCollection(typeof(string));

            return base.ParseFilter();
        }
    }

    private sealed class ScopeAccessingFilterParser(IResourceFactory resourceFactory)
        : FilterParser(resourceFactory)
    {
        protected override void Tokenize(string source)
        {
            // Fails because there is no scope active yet.
            _ = CurrentScope;

            base.Tokenize(source);
        }
    }
}
