using System.Net;
using System.Text.Json;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.SparseFieldSets;

/// <summary>
/// Verifies that selecting a part of a compound attribute does not fail when the compound attribute (or a nested one) is NULL in the database.
/// </summary>
public sealed class SparseFieldSetCompoundNullTests : IClassFixture<IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;
    private readonly QueryStringFakers _fakers = new();

    public SparseFieldSetCompoundNullTests(IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<CalendarsController>();
    }

    [Fact]
    public async Task Can_select_nested_leaf_when_compound_is_null()
    {
        // Arrange
        List<Calendar> calendars = _fakers.Calendar.GenerateList(2);
        calendars[0].Workweek = null;

        calendars[1].Workweek = new Workweek
        {
            FirstDay = DayOfWeek.Tuesday,
            WorkDays = [DayOfWeek.Tuesday]
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Calendar>();
            dbContext.Calendars.AddRange(calendars);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/calendars?fields[calendars]=workweek.firstDay&sort=id";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);

        responseDocument.Data.ManyValue[0].Attributes.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].GetAttribute("workweek").ValueKind.Should().Be(JsonValueKind.Null);

        responseDocument.Data.ManyValue[1].Attributes.Should().HaveCount(1);
        responseDocument.Data.ManyValue[1].GetAttribute("workweek").GetMemberNames().Should().Equal("firstDay");
    }

    [Fact]
    public async Task Can_select_deeply_nested_leaf_when_nested_compound_is_null()
    {
        // Arrange
        List<Calendar> calendars = _fakers.Calendar.GenerateList(2);

        calendars[0].Workweek = new Workweek
        {
            FirstDay = DayOfWeek.Monday,
            OfficeHours = null
        };

        calendars[1].Workweek = new Workweek
        {
            FirstDay = DayOfWeek.Tuesday,
            OfficeHours = new OfficeHours
            {
                OpensAt = new TimeOnly(9, 0),
                ClosesAt = new TimeOnly(17, 0)
            }
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Calendar>();
            dbContext.Calendars.AddRange(calendars);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/calendars?fields[calendars]=workweek.officeHours.opensAt&sort=id";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);

        JsonElement firstWorkweek = responseDocument.Data.ManyValue[0].GetAttribute("workweek");
        firstWorkweek.GetMemberNames().Should().Equal("officeHours");
        firstWorkweek.GetProperty("officeHours").ValueKind.Should().Be(JsonValueKind.Null);

        JsonElement secondWorkweek = responseDocument.Data.ManyValue[1].GetAttribute("workweek");
        secondWorkweek.GetMemberNames().Should().Equal("officeHours");
        secondWorkweek.GetProperty("officeHours").GetMemberNames().Should().Equal("opensAt");
    }

    [Fact]
    public async Task Can_select_scalar_collection_inside_compound_whole()
    {
        // Arrange
        Calendar calendar = _fakers.Calendar.GenerateOne();

        calendar.Workweek = new Workweek
        {
            FirstDay = DayOfWeek.Monday,
            WorkDays =
            [
                DayOfWeek.Monday,
                DayOfWeek.Wednesday
            ]
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Calendar>();
            dbContext.Calendars.Add(calendar);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/calendars?fields[calendars]=workweek.workDays";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);

        JsonElement workweek = responseDocument.Data.ManyValue[0].GetAttribute("workweek");
        workweek.GetMemberNames().Should().Equal("workDays");
        workweek.GetProperty("workDays").GetArrayLength().Should().Be(2);
    }
}
