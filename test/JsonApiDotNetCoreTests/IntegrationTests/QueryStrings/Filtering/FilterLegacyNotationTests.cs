using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.Filtering;

public sealed class FilterLegacyNotationTests : IClassFixture<IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;
    private readonly QueryStringFakers _fakers = new();

    public FilterLegacyNotationTests(IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<AppointmentsController>();
        testContext.UseController<CalendarsController>();

        var options = (JsonApiOptions)testContext.App.Services.GetRequiredService<IJsonApiOptions>();
        options.EnableLegacyFilterNotation = true;
    }

    [Theory]
    [InlineData("isnotnull:", "0")]
    [InlineData("isnull:", "12")]
    public async Task Can_filter_on_null_check_of_compound_attribute_through_to_one_relationship(string filterValue, string expectedRows)
    {
        // Arrange
        List<Appointment> appointments = _fakers.Appointment.GenerateList(3);

        appointments[0].Calendar = _fakers.Calendar.GenerateOne();
        appointments[0].Calendar!.Workweek = new Workweek();

        appointments[1].Calendar = _fakers.Calendar.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Appointment>();
            dbContext.Appointments.AddRange(appointments);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/appointments?filter[calendar.workweek]={filterValue}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        string[] expectedIds = expectedRows.Select(row => appointments[row - '0'].StringId!).ToArray();
        responseDocument.Data.ManyValue.Should().HaveCount(expectedIds.Length);
        responseDocument.Data.ManyValue.Select(resource => resource.Id).Should().BeEquivalentTo(expectedIds);
    }

    [Fact]
    public async Task Can_filter_on_nested_attribute_of_compound_attribute()
    {
        // Arrange
        List<Appointment> appointments = _fakers.Appointment.GenerateList(2);

        appointments[0].Location = new AppointmentLocation
        {
            Address = new AppointmentAddress
            {
                City = "Utrecht"
            }
        };

        appointments[1].Location = new AppointmentLocation
        {
            Address = new AppointmentAddress
            {
                City = "Breda"
            }
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Appointment>();
            dbContext.Appointments.AddRange(appointments);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/appointments?filter[location.address.city]=Utrecht";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(appointments[0].StringId);
    }

    [Fact]
    public async Task Can_filter_with_expression_in_scope_of_to_many_relationship()
    {
        // Arrange
        Calendar calendar = _fakers.Calendar.GenerateOne();
        calendar.PinnedAppointments = _fakers.Appointment.GenerateSet(2);

        calendar.PinnedAppointments.ElementAt(0).Location = new AppointmentLocation
        {
            Name = "Office"
        };

        calendar.PinnedAppointments.ElementAt(1).Location = new AppointmentLocation
        {
            Name = "Home"
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Calendar>();
            dbContext.Calendars.Add(calendar);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/calendars?include=pinnedAppointments&filter[pinnedAppointments]=expr:equals(location.name,'Home')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(calendar.StringId);

        responseDocument.Included.Should().HaveCount(1);
        responseDocument.Included[0].Id.Should().Be(calendar.PinnedAppointments.ElementAt(1).StringId);
    }

    [Theory]
    [InlineData("filter[calendar]", "expr:equals(timeZone,'UTC')", "To-many relationship 'calendar' does not exist on resource type 'appointments'.")]
    [InlineData("filter[calendar.workweek]", "expr:equals(firstDay,'Monday')", "To-many relationship 'workweek' does not exist on resource type 'calendars'.")]
    [InlineData("filter[calendar]", "UTC",
        "Filtering on relationship 'calendar' is only supported using 'isnull:' or 'isnotnull:'. To filter by related attributes, use 'filter[calendar.id]=123' or use expression syntax.")]
    public async Task Cannot_filter_on_to_one_relationship_with_expression_or_value(string parameterName, string parameterValue, string errorMessage)
    {
        // Arrange
        string route = $"/appointments?{parameterName}={parameterValue}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        ErrorObject error = responseDocument.Errors.Should().ContainSingle().Which;
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified filter is invalid.");
        error.Detail.Should().Be(errorMessage);
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be(parameterName);
    }
}
