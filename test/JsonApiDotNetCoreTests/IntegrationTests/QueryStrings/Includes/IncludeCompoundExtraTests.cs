using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.Includes;

/// <summary>
/// Additional scenarios for includes and relationship endpoints where the resource types involved have compound attributes.
/// </summary>
public sealed class IncludeCompoundExtraTests : IClassFixture<IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;
    private readonly QueryStringFakers _fakers = new();

    public IncludeCompoundExtraTests(IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<AppointmentsController>();
        testContext.UseController<CalendarsController>();

        var options = (JsonApiOptions)testContext.App.Services.GetRequiredService<IJsonApiOptions>();
        options.MaximumIncludeDepth = null;
        options.DefaultPageSize = new PageSize(10);
        options.MaximumPageSize = null;
        options.MaximumPageNumber = null;
    }

    [Fact]
    public async Task Can_include_chain_with_cyclic_dependency_through_to_many_on_compound_top()
    {
        // Arrange
        Calendar calendar = _fakers.Calendar.GenerateOne();
        calendar.PinnedAppointments = _fakers.Appointment.GenerateSet(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Calendar>();
            dbContext.Calendars.Add(calendar);
            await dbContext.SaveChangesAsync();

            foreach (Appointment appointment in calendar.PinnedAppointments)
            {
                appointment.Calendar = calendar;
            }

            await dbContext.SaveChangesAsync();
        });

        const string route = "/calendars?include=pinnedAppointments.calendar.pinnedAppointments.calendar";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(calendar.StringId);

        responseDocument.Included.Should().HaveCount(2);
        responseDocument.Included.Should().OnlyContain(resource => resource.Type == "appointments");
        responseDocument.GetIncludedIds().Should().BeEquivalentTo(calendar.PinnedAppointments.Select(appointment => appointment.StringId));
    }

    [Fact]
    public async Task Can_sort_relationship_endpoint_of_compound_top()
    {
        // Arrange
        Calendar calendar = _fakers.Calendar.GenerateOne();
        calendar.PinnedAppointments = _fakers.Appointment.GenerateSet(3);
        Appointment[] appointments = calendar.PinnedAppointments.ToArray();
        appointments[0].Title = "b";
        appointments[1].Title = "c";
        appointments[2].Title = "a";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Calendars.Add(calendar);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/calendars/{calendar.StringId}/relationships/pinnedAppointments?sort=-title";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.GetPrimaryIds().Should().Equal(appointments[1].StringId, appointments[0].StringId, appointments[2].StringId);
    }
}
