using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.QueryStringParameters;

public sealed class SparseFieldSetParseTests : BaseParseTests
{
    private readonly SparseFieldSetQueryStringParameterReader _reader;

    public SparseFieldSetParseTests()
    {
        var scopeParser = new SparseFieldTypeParser(ResourceGraph);
        var sparseFieldSetParser = new SparseFieldSetParser();
        _reader = new SparseFieldSetQueryStringParameterReader(scopeParser, sparseFieldSetParser, Request, ResourceGraph);
    }

    [Theory]
    [InlineData("fields", false)]
    [InlineData("fields[articles]", true)]
    [InlineData("fields[]", true)]
    [InlineData("fieldset", false)]
    [InlineData("fields[", false)]
    [InlineData("fields]", false)]
    public void Reader_Supports_Parameter_Name(string parameterName, bool expectCanParse)
    {
        // Act
        bool canParse = _reader.CanRead(parameterName);

        // Assert
        canParse.Should().Be(expectCanParse);
    }

    [Theory]
    [InlineData(JsonApiQueryStringParameters.Fields, false)]
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
    [InlineData("fields^", "[ expected.")]
    [InlineData("fields^]", "[ expected.")]
    [InlineData("fields[^", "Resource type expected.")]
    [InlineData("fields[^]", "Resource type expected.")]
    [InlineData("fields[^ ]", "Unexpected whitespace.")]
    [InlineData("fields[^owner]", "Resource type 'owner' does not exist.")]
    [InlineData("fields[^owner.posts]", "Resource type 'owner' does not exist.")]
    [InlineData("fields[^unknown]", "Resource type 'unknown' does not exist.")]
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
        error.Title.Should().Be("The specified fieldset is invalid.");
        error.Detail.Should().Be($"{errorMessage} {parameterNameSource}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be(parameterNameSource.Text);
    }

    [Theory]
    [InlineData("fields[blogPosts]", "^ ", "Unexpected whitespace.")]
    [InlineData("fields[blogPosts]", "^some", "Field 'some' does not exist on resource type 'blogPosts'.")]
    [InlineData("fields[blogPosts]", "id,^owner.name", "Field 'owner' does not exist on resource type 'blogPosts'.")]
    [InlineData("fields[blogPosts]", "id^(", ", expected.")]
    [InlineData("fields[blogPosts]", "id,^", "Field name expected.")]
    [InlineData("fields[blogPosts]", "author.^id,",
        "Field chain on resource type 'blogPosts' failed to match the pattern: zero or more compound attributes, followed by a field. " +
        "End of field chain expected.")]
    // Compound attributes
    [InlineData("fields[appointments]", "keywords.^x",
        "Field chain on resource type 'appointments' failed to match the pattern: zero or more compound attributes, followed by a field. " +
        "End of field chain expected.")]
    [InlineData("fields[appointments]", "calendar.^title",
        "Field chain on resource type 'appointments' failed to match the pattern: zero or more compound attributes, followed by a field. " +
        "End of field chain expected.")]
    [InlineData("fields[appointments]", "location.address.city.^x",
        "Field chain on resource type 'appointments' failed to match the pattern: zero or more compound attributes, followed by a field. " +
        "End of field chain expected.")]
    [InlineData("fields[calendars]", "workweek.workDays.^x",
        "Field chain on resource type 'calendars' failed to match the pattern: zero or more compound attributes, followed by a field. " +
        "End of field chain expected.")]
    [InlineData("fields[calendars]", "mostRecentAppointment.^location",
        "Field chain on resource type 'calendars' failed to match the pattern: zero or more compound attributes, followed by a field. " +
        "End of field chain expected.")]
    [InlineData("fields[appointments]", "location.^unknown", "Field 'unknown' does not exist on compound type 'AppointmentLocation'.")]
    [InlineData("fields[appointments]", "attendees.^unknown", "Field 'unknown' does not exist on compound type 'Attendee'.")]
    [InlineData("fields[appointments]", "^unknown.x", "Field 'unknown' does not exist on resource type 'appointments'.")]
    [InlineData("fields[appointments]", "location.^", "Field name expected.")]
    [InlineData("fields[calendars]", "workweek.^secret", "Retrieving attribute 'secret' is not allowed.")]
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
        error.Title.Should().Be("The specified fieldset is invalid.");
        error.Detail.Should().Be($"{errorMessage} {parameterValueSource}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be(parameterName);
    }

    [Theory]
    [InlineData("fields[blogPosts]", "caption,url,author", "blogPosts", "author,caption,url")]
    [InlineData("fields[blogPosts]", "author,comments,labels", "blogPosts", "author,comments,labels")]
    [InlineData("fields[blogs]", "id", "blogs", "id")]
    [InlineData("fields[blogs]", "", "blogs", "")]
    [InlineData("fields[appointments]", "", "appointments", "")]
    [InlineData("fields[blogs]", "tags", "blogs", "tags")]
    // Compound attributes
    [InlineData("fields[appointments]", "location", "appointments", "location")]
    [InlineData("fields[appointments]", "location.address.city", "appointments", "location.address.city")]
    [InlineData("fields[appointments]", "location.address.city,location.address.street", "appointments", "location.address.city,location.address.street")]
    [InlineData("fields[appointments]", "location.address.city,location.name", "appointments", "location.address.city,location.name")]
    [InlineData("fields[appointments]", "cost.amount,cost.currency", "appointments", "cost.amount,cost.currency")]
    [InlineData("fields[appointments]", "attendees", "appointments", "attendees")]
    [InlineData("fields[appointments]", "attendees.name", "appointments", "attendees.name")]
    [InlineData("fields[appointments]", "keywords", "appointments", "keywords")]
    [InlineData("fields[appointments]", "calendar", "appointments", "calendar")]
    [InlineData("fields[calendars]", "workweek.officeHours.opensAt", "calendars", "workweek.officeHours.opensAt")]
    [InlineData("fields[calendars]", "workweek.officeHours", "calendars", "workweek.officeHours")]
    [InlineData("fields[calendars]", "workweek.workDays", "calendars", "workweek.workDays")]
    [InlineData("fields[calendars]", "workweek.internalNote", "calendars", "workweek.internalNote")]
    [InlineData("fields[calendars]", "audit.createdBy", "calendars", "audit.createdBy")]
    [InlineData("fields[calendars]", "holidays.name", "calendars", "holidays.name")]
    [InlineData("fields[calendars]", "mostRecentAppointment", "calendars", "mostRecentAppointment")]
    // Merging of a whole compound with its children, in both orders
    [InlineData("fields[appointments]", "location.address.city,location.address", "appointments", "location.address")]
    [InlineData("fields[appointments]", "location.address,location.address.city", "appointments", "location.address")]
    [InlineData("fields[appointments]", "cost.amount,cost", "appointments", "cost")]
    [InlineData("fields[calendars]", "holidays.date,holidays", "calendars", "holidays")]
    // Duplicates are removed
    [InlineData("fields[appointments]", "location.address.city,location.address.city", "appointments", "location.address.city")]
    [InlineData("fields[calendars]", "holidays.name,holidays.name", "calendars", "holidays.name")]
    public void Reader_Read_Succeeds(string parameterName, string parameterValue, string resourceTypeExpected, string valueExpected)
    {
        // Act
        _reader.Read(parameterName, parameterValue);

        // Assert
        ResourceTypeExpressionInScope constraint = _reader.GetConstraints().Cast<ResourceTypeExpressionInScope>().ToArray().Single();
        constraint.ResourceType.PublicName.Should().Be(resourceTypeExpected);
        constraint.Expression.ToString().Should().Be(valueExpected);
    }
}
