using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.Filtering;

/// <summary>
/// Verifies that a null collection, a null collection inside a compound, and a collection below a null compound all behave as an empty collection in
/// filters, for scalar and compound collection elements, at the root and nested in compounds.
/// </summary>
public sealed class FilterCompoundNullTests : IClassFixture<IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;
    private readonly QueryStringFakers _fakers = new();

    public FilterCompoundNullTests(IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<CalendarsController>();
        testContext.UseController<AppointmentsController>();
    }

    public static TheoryData<string, string> CalendarScalarCollectionExpressions => CreateExpressions(
    [
        ("workweek.extraDays", "equals(_,'Tuesday')", "equals(_,'Friday')"),
        ("workweek.overtime.days", "equals(_,'Monday')", "equals(_,'Friday')")
    ]);

    public static TheoryData<string, string> CalendarCompoundCollectionExpressions => CreateExpressions(
    [
        ("workweek.extraBreaks", "equals(opensAt,'09:00:00')", "equals(opensAt,'10:00:00')"),
        ("workweek.overtime.windows", "equals(opensAt,'09:00:00')", "equals(opensAt,'10:00:00')")
    ]);

    public static TheoryData<string, string> AppointmentCollectionExpressions => CreateExpressions(
    [
        ("keywords", "equals(_,'x')", "equals(_,'y')"),
        ("waitingList", "equals(name,'Alice')", "equals(name,'Bob')"),
        ("calendar.workweek.extraDays", "equals(_,'Tuesday')", "equals(_,'Friday')"),
        ("calendar.workweek.overtime.windows", "equals(opensAt,'09:00:00')", "equals(opensAt,'10:00:00')")
    ]);

    private static TheoryData<string, string> CreateExpressions((string Path, string Matching, string NonMatching)[] paths)
    {
        var data = new TheoryData<string, string>();

        foreach ((string path, string matching, string nonMatching) in paths)
        {
            data.Add($"has({path})", "3");
            data.Add($"not(has({path}))", "012");
            data.Add($"has({path},{matching})", "3");
            data.Add($"not(has({path},{matching}))", "012");
            data.Add($"has({path},{nonMatching})", "");
            data.Add($"not(has({path},{nonMatching}))", "0123");
            data.Add($"equals(count({path}),'0')", "012");
            data.Add($"not(equals(count({path}),'0'))", "3");
            data.Add($"greaterThan(count({path}),'0')", "3");
            data.Add($"lessThan(count({path}),'1')", "012");
            data.Add($"and(not(has({path})),not(has({path},{nonMatching})))", "012");
            data.Add($"or(has({path}),equals(count({path}),'0'))", "0123");
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(CalendarScalarCollectionExpressions))]
    public async Task Null_scalar_collections_count_as_empty_in_calendar_filter(string filterExpression, string expectedRows)
    {
        // Arrange
        List<Calendar> calendars = NullPathTestData.CreateCalendars(_fakers);

        // Act and assert
        await AssertCalendarFilterAsync(calendars, filterExpression, expectedRows);
    }

    [Theory]
    [MemberData(nameof(CalendarCompoundCollectionExpressions))]
    public async Task Null_compound_collections_count_as_empty_in_calendar_filter(string filterExpression, string expectedRows)
    {
        // Arrange
        List<Calendar> calendars = NullPathTestData.CreateCalendars(_fakers, true);

        // Act and assert
        await AssertCalendarFilterAsync(calendars, filterExpression, expectedRows);
    }

    [Fact(Skip = "Npgsql: A null scalar collection nested in a JSON compound is stored as JSON null, which fails in jsonb_array_length with " +
        "'cannot get array length of a scalar' and cannot be detected through a null check.")]
    public async Task Json_null_scalar_collection_nested_in_compound_counts_as_empty()
    {
        // Arrange
        List<Calendar> calendars = NullPathTestData.CreateCalendars(_fakers, true);

        // Act and assert
        await AssertCalendarFilterAsync(calendars, "not(has(workweek.extraDays))", "012");
    }

    private async Task AssertCalendarFilterAsync(List<Calendar> calendars, string filterExpression, string expectedRows)
    {
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Calendar>();
            dbContext.Calendars.AddRange(calendars);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/calendars?filter={filterExpression}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        string[] expectedIds = expectedRows.Select(row => calendars[row - '0'].StringId!).ToArray();
        responseDocument.GetPrimaryIds().Should().BeEquivalentTo(expectedIds);
    }
    [Theory]
    [MemberData(nameof(AppointmentCollectionExpressions))]
    public async Task Null_collections_count_as_empty_in_appointment_filter(string filterExpression, string expectedRows)
    {
        // Arrange
        List<Calendar> calendars = NullPathTestData.CreateCalendars(_fakers);
        List<Appointment> appointments = NullPathTestData.CreateAppointments(_fakers, calendars);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Calendar>();
            await dbContext.ClearTableAsync<Appointment>();
            dbContext.Appointments.AddRange(appointments);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/appointments?filter={filterExpression}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        string[] expectedIds = expectedRows.Select(row => appointments[row - '0'].StringId!).ToArray();
        responseDocument.GetPrimaryIds().Should().BeEquivalentTo(expectedIds);
    }

    [Fact]
    public async Task Can_filter_on_nested_attribute_through_relationship()
    {
        // Arrange
        List<Calendar> calendars = NullPathTestData.CreateCalendars(_fakers);
        calendars[1].Workweek!.FirstDay = DayOfWeek.Wednesday;

        List<Appointment> appointments = NullPathTestData.CreateAppointments(_fakers, calendars);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Calendar>();
            await dbContext.ClearTableAsync<Appointment>();
            dbContext.Appointments.AddRange(appointments);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/appointments?filter=equals(calendar.workweek.firstDay,'Wednesday')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.GetPrimaryIds().Should().Equal(appointments[1].StringId);
    }
    [Theory]
    [InlineData("has(keywords)", "3")]
    [InlineData("not(has(keywords))", "012")]
    [InlineData("equals(count(waitingList),'0')", "012")]
    [InlineData("not(has(calendar.workweek.overtime.days,equals(_,'Monday')))", "012")]
    public async Task Null_collections_count_as_empty_in_include_filter(string filterExpression, string expectedRows)
    {
        // Arrange
        List<Calendar> calendars = NullPathTestData.CreateCalendars(_fakers);
        List<Appointment> appointments = NullPathTestData.CreateAppointments(_fakers, calendars);
        Calendar owner = _fakers.Calendar.GenerateOne();
        owner.PinnedAppointments = appointments.ToHashSet();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Calendar>();
            await dbContext.ClearTableAsync<Appointment>();
            dbContext.Calendars.Add(owner);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/calendars/{owner.StringId}?include=pinnedAppointments&filter[pinnedAppointments]={filterExpression}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        string[] expectedIds = expectedRows.Select(row => appointments[row - '0'].StringId!).ToArray();
        responseDocument.GetIncludedIds().Should().BeEquivalentTo(expectedIds);
    }

    [Theory]
    [InlineData("has(keywords)", "3")]
    [InlineData("not(has(keywords))", "012")]
    [InlineData("equals(count(waitingList),'0')", "012")]
    public async Task Null_collections_count_as_empty_at_secondary_endpoint(string filterExpression, string expectedRows)
    {
        // Arrange
        List<Calendar> calendars = NullPathTestData.CreateCalendars(_fakers);
        List<Appointment> appointments = NullPathTestData.CreateAppointments(_fakers, calendars);
        Calendar owner = _fakers.Calendar.GenerateOne();
        owner.PinnedAppointments = appointments.ToHashSet();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Calendar>();
            await dbContext.ClearTableAsync<Appointment>();
            dbContext.Calendars.Add(owner);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/calendars/{owner.StringId}/pinnedAppointments?filter={filterExpression}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        string[] expectedIds = expectedRows.Select(row => appointments[row - '0'].StringId!).ToArray();
        responseDocument.GetPrimaryIds().Should().BeEquivalentTo(expectedIds);
    }
}
