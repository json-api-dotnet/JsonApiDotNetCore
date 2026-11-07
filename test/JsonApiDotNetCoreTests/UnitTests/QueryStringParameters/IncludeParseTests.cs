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

public sealed class IncludeParseTests : BaseParseTests
{
    private readonly IncludeQueryStringParameterReader _reader;

    public IncludeParseTests()
    {
        var options = new JsonApiOptions();
        var parser = new IncludeParser(options);
        _reader = new IncludeQueryStringParameterReader(parser, Request, ResourceGraph);
    }

    [Theory]
    [InlineData("include", true)]
    [InlineData("include[some]", false)]
    [InlineData("includes", false)]
    public void Reader_Supports_Parameter_Name(string parameterName, bool expectCanParse)
    {
        // Act
        bool canParse = _reader.CanRead(parameterName);

        // Assert
        canParse.Should().Be(expectCanParse);
    }

    [Theory]
    [InlineData(JsonApiQueryStringParameters.Include, false)]
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
    [InlineData("includes", "^ ", "Unexpected whitespace.")]
    [InlineData("includes", "^,", "Relationship name expected.")]
    [InlineData("includes", "posts,^", "Relationship name expected.")]
    [InlineData("includes", "posts.^", "Relationship name expected.")]
    [InlineData("includes", "posts^[", ", expected.")]
    [InlineData("includes", "^title", "Relationship 'title' does not exist on resource type 'blogs'.")]
    [InlineData("includes", "posts.comments.^publishTime,", "Relationship 'publishTime' does not exist on resource type 'comments'.")]
    [InlineData("includes", "owner.person.children.^unknown", "Relationship 'unknown' does not exist on resource type 'humans' or any of its derived types.")]
    [InlineData("includes", "owner.person.friends.^unknown", "Relationship 'unknown' does not exist on resource type 'humans' or any of its derived types.")]
    [InlineData("includes", "owner.person.sameGenderFriends.^unknown", "Relationship 'unknown' does not exist on any of the resource types 'men', 'women'.")]
    public void Reader_Read_Fails(string parameterName, string parameterValue, string errorMessage)
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
        error.Title.Should().Be("The specified include is invalid.");
        error.Detail.Should().Be($"{errorMessage} {parameterValueSource}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be(parameterName);
    }

    [Theory]
    [InlineData("includes", "", "", "")]
    [InlineData("includes", "owner", "owner", "blogs:owner")]
    [InlineData("includes", "posts", "posts", "blogs:posts")]
    [InlineData("includes", "owner.posts", "owner.posts", "blogs:owner.webAccounts:posts")]
    [InlineData("includes", "posts.author", "posts.author", "blogs:posts.blogPosts:author")]
    [InlineData("includes", "posts.comments", "posts.comments", "blogs:posts.blogPosts:comments")]
    [InlineData("includes", "posts,posts.comments", "posts.comments", "blogs:posts.blogPosts:comments")]
    [InlineData("includes", "posts,posts.labels,posts.comments", "posts.comments,posts.labels", "blogs:posts.blogPosts:comments,blogs:posts.blogPosts:labels")]
    [InlineData("includes", "owner.person.children.husband", "owner.person.children.husband",
        "blogs:owner.webAccounts:person.men:children.women:husband,blogs:owner.webAccounts:person.women:children.women:husband")]
    [InlineData("includes", "owner.person.wife,owner.person.husband", "owner.person.husband,owner.person.wife",
        "blogs:owner.webAccounts:person.men:wife,blogs:owner.webAccounts:person.women:husband")]
    [InlineData("includes", "owner.person.father.children.wife", "owner.person.father.children.wife",
        "blogs:owner.webAccounts:person.men:father.men:children.men:wife,blogs:owner.webAccounts:person.women:father.men:children.men:wife")]
    [InlineData("includes", "owner.person.friends", "owner.person.friends",
        "blogs:owner.webAccounts:person.men:friends,blogs:owner.webAccounts:person.women:friends")]
    [InlineData("includes", "owner.person.friends.friends", "owner.person.friends.friends",
        "blogs:owner.webAccounts:person.men:friends.men:friends,blogs:owner.webAccounts:person.men:friends.women:friends," +
        "blogs:owner.webAccounts:person.women:friends.men:friends,blogs:owner.webAccounts:person.women:friends.women:friends")]
    public void Reader_Read_Succeeds(string parameterName, string parameterValue, string stringExpected, string fullStringExpected)
    {
        // Act
        _reader.Read(parameterName, parameterValue);

        // Assert
        RelationshipChainExpressionInScope[] constraints = _reader.GetConstraints().Cast<RelationshipChainExpressionInScope>().ToArray();
        ResourceFieldChainExpression? scope = constraints.Select(expressionInScope => expressionInScope.Scope).Single();
        scope.Should().BeNull();

        QueryExpression value = constraints.Select(expressionInScope => expressionInScope.Expression).Single();
        value.ToString().Should().Be(stringExpected);
        value.ToFullString().Should().Be(fullStringExpected);
    }

    [Theory]
    [InlineData("calendars", "appointments.calendar", "appointments.calendar", "calendars:appointments.appointments:calendar")]
    [InlineData("calendars", "appointments,mostRecentAppointment", "appointments,mostRecentAppointment",
        "calendars:appointments,calendars:mostRecentAppointment")]
    [InlineData("appointments", "calendar,reminders", "calendar,reminders", "appointments:calendar,appointments:reminders")]
    public void Reader_Read_Succeeds_On_Compound_Types(string resourceTypeName, string parameterValue, string stringExpected, string fullStringExpected)
    {
        // Arrange
        IncludeQueryStringParameterReader reader = CreateReader(resourceTypeName);

        // Act
        reader.Read("include", parameterValue);

        // Assert
        RelationshipChainExpressionInScope[] constraints = reader.GetConstraints().Cast<RelationshipChainExpressionInScope>().ToArray();
        ResourceFieldChainExpression? scope = constraints.Select(expressionInScope => expressionInScope.Scope).Single();
        scope.Should().BeNull();

        QueryExpression value = constraints.Select(expressionInScope => expressionInScope.Expression).Single();
        value.ToString().Should().Be(stringExpected);
        value.ToFullString().Should().Be(fullStringExpected);
    }

    [Theory]
    // Attributes are never relationships, whether compound, collection or scalar
    [InlineData("calendars", "^workweek", "Relationship 'workweek' does not exist on resource type 'calendars'.")]
    [InlineData("calendars", "^holidays", "Relationship 'holidays' does not exist on resource type 'calendars'.")]
    [InlineData("calendars", "^audit", "Relationship 'audit' does not exist on resource type 'calendars'.")]
    [InlineData("calendars", "^workweek.officeHours", "Relationship 'workweek' does not exist on resource type 'calendars'.")]
    [InlineData("calendars", "mostRecentAppointment.^location", "Relationship 'location' does not exist on resource type 'appointments'.")]
    [InlineData("calendars", "mostRecentAppointment.^attendees", "Relationship 'attendees' does not exist on resource type 'appointments'.")]
    [InlineData("calendars", "mostRecentAppointment.calendar.^workweek", "Relationship 'workweek' does not exist on resource type 'calendars'.")]
    [InlineData("calendars", "appointments.^attendees", "Relationship 'attendees' does not exist on resource type 'appointments'.")]
    [InlineData("calendars", "appointments.^keywords", "Relationship 'keywords' does not exist on resource type 'appointments'.")]
    [InlineData("appointments", "^location", "Relationship 'location' does not exist on resource type 'appointments'.")]
    [InlineData("appointments", "^location.address", "Relationship 'location' does not exist on resource type 'appointments'.")]
    [InlineData("appointments", "^cost", "Relationship 'cost' does not exist on resource type 'appointments'.")]
    [InlineData("appointments", "^keywords", "Relationship 'keywords' does not exist on resource type 'appointments'.")]
    [InlineData("appointments", "calendar.^workweek", "Relationship 'workweek' does not exist on resource type 'calendars'.")]
    [InlineData("appointments", "calendar.^audit", "Relationship 'audit' does not exist on resource type 'calendars'.")]
    [InlineData("blogs", "^tags", "Relationship 'tags' does not exist on resource type 'blogs'.")]
    [InlineData("blogs", "owner.^address", "Relationship 'address' does not exist on resource type 'webAccounts'.")]
    [InlineData("blogs", "posts.^address", "Relationship 'address' does not exist on resource type 'blogPosts'.")]
    public void Reader_Read_Fails_On_Compound_Types(string resourceTypeName, string parameterValue, string errorMessage)
    {
        // Arrange
        IncludeQueryStringParameterReader reader = CreateReader(resourceTypeName);
        var parameterValueSource = new MarkedText(parameterValue, '^');

        // Act
        Action action = () => reader.Read("include", parameterValueSource.Text);

        // Assert
        InvalidQueryStringParameterException exception = action.Should().ThrowExactly<InvalidQueryStringParameterException>().And;

        exception.ParameterName.Should().Be("include");
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified include is invalid.");
        error.Detail.Should().Be($"{errorMessage} {parameterValueSource}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("include");
    }

    private IncludeQueryStringParameterReader CreateReader(string resourceTypeName)
    {
        Request.PrimaryResourceType = ResourceGraph.GetResourceType(resourceTypeName);

        var options = new JsonApiOptions();
        var parser = new IncludeParser(options);
        return new IncludeQueryStringParameterReader(parser, Request, ResourceGraph);
    }
}
