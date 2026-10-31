using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.Sorting;

/// <summary>
/// Verifies that a null collection, a null collection inside a compound, and a collection below a null compound all sort as an empty collection when
/// sorting by count.
/// </summary>
public sealed class SortCompoundNullTests : IClassFixture<IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;
    private readonly QueryStringFakers _fakers = new();

    public SortCompoundNullTests(IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<CalendarsController>();
        testContext.UseController<AppointmentsController>();
    }

    [Theory]
    [InlineData("workweek.extraDays", false)]
    [InlineData("workweek.overtime.days", false)]
    [InlineData("workweek.extraBreaks", true)]
    [InlineData("workweek.overtime.windows", true)]
    public async Task Can_sort_calendars_by_count_of_nested_collection_with_null_path(string path, bool nullScalarCollections)
    {
        // Arrange
        List<Calendar> calendars = NullPathTestData.CreateCalendars(_fakers, nullScalarCollections);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Calendar>();
            dbContext.Calendars.AddRange(calendars);
            await dbContext.SaveChangesAsync();
        });

        string ascendingRoute = $"/calendars?sort=count({path}),id";
        string descendingRoute = $"/calendars?sort=-count({path}),id";

        // Act
        (HttpResponseMessage ascendingResponse, Document ascendingDocument) = await _testContext.ExecuteGetAsync<Document>(ascendingRoute);
        (HttpResponseMessage descendingResponse, Document descendingDocument) = await _testContext.ExecuteGetAsync<Document>(descendingRoute);

        // Assert
        ascendingResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        ascendingDocument.GetPrimaryIds().Should().Equal(calendars[0].StringId, calendars[1].StringId, calendars[2].StringId, calendars[3].StringId);

        descendingResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        descendingDocument.GetPrimaryIds().Should().Equal(calendars[3].StringId, calendars[0].StringId, calendars[1].StringId, calendars[2].StringId);
    }

    [Fact(Skip = "Npgsql: A null scalar collection nested in a JSON compound is stored as JSON null, which fails in jsonb_array_length with " +
        "'cannot get array length of a scalar' and cannot be detected through a null check.")]
    public async Task Json_null_scalar_collection_nested_in_compound_sorts_as_empty()
    {
        // Arrange
        List<Calendar> calendars = NullPathTestData.CreateCalendars(_fakers, true);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Calendar>();
            dbContext.Calendars.AddRange(calendars);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/calendars?sort=count(workweek.extraDays),id";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        responseDocument.GetPrimaryIds().Should().Equal(calendars[0].StringId, calendars[1].StringId, calendars[2].StringId, calendars[3].StringId);
    }

    [Theory]
    [InlineData("keywords")]
    [InlineData("waitingList")]
    [InlineData("calendar.workweek.extraDays")]
    [InlineData("calendar.workweek.overtime.windows")]
    public async Task Can_sort_appointments_by_count_of_collection_with_null_path(string path)
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

        string ascendingRoute = $"/appointments?sort=count({path}),id";
        string descendingRoute = $"/appointments?sort=-count({path}),id";

        // Act
        (HttpResponseMessage ascendingResponse, Document ascendingDocument) = await _testContext.ExecuteGetAsync<Document>(ascendingRoute);
        (HttpResponseMessage descendingResponse, Document descendingDocument) = await _testContext.ExecuteGetAsync<Document>(descendingRoute);

        // Assert
        ascendingResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        ascendingDocument.GetPrimaryIds().Should().Equal(appointments[0].StringId, appointments[1].StringId, appointments[2].StringId, appointments[3].StringId);

        descendingResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        descendingDocument.GetPrimaryIds().Should().Equal(appointments[3].StringId, appointments[0].StringId, appointments[1].StringId, appointments[2].StringId);
    }

    [Fact]
    public async Task Can_sort_included_collection_by_count_with_null_collections()
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

        string route = $"/calendars/{owner.StringId}/pinnedAppointments?sort=-count(keywords),id";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        responseDocument.GetPrimaryIds().Should().Equal(appointments[3].StringId, appointments[0].StringId, appointments[1].StringId, appointments[2].StringId);
    }
}