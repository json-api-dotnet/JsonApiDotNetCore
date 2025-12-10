using System.Net;
using FluentAssertions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.Includes;

public sealed class DisablePaginationOnRelationshipTests : IClassFixture<IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;
    private readonly QueryStringFakers _fakers = new();

    public DisablePaginationOnRelationshipTests(IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<AppointmentsController>();
        testContext.UseController<CalendarsController>();

        testContext.ConfigureServices(services =>
        {
            services.AddResourceDefinition<ReminderDefinition>();
            services.AddSingleton<PaginationToggle>();
        });

        var paginationToggle = testContext.Factory.Services.GetRequiredService<PaginationToggle>();
        paginationToggle.IsEnabled = false;
        paginationToggle.IsCalled = false;

        var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.DefaultPageSize = new PageSize(5);
        options.UseRelativeLinks = true;
        options.IncludeTotalResourceCount = true;
    }

    [Fact]
    public async Task Can_include_in_primary_resources()
    {
        // Arrange
        Appointment appointment = _fakers.Appointment.GenerateOne();
        appointment.Reminders = _fakers.Reminder.GenerateList(7);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Appointment>();
            dbContext.Appointments.Add(appointment);
            await dbContext.SaveChangesAsync();
        });

        const string route = "appointments?include=reminders";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Type.Should().Be("appointments");
        responseDocument.Data.ManyValue[0].Id.Should().Be(appointment.StringId);

        responseDocument.Data.ManyValue[0].Relationships.Should().ContainKey("reminders").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();
            value.Data.ManyValue.Should().HaveCount(7);

            value.Links.Should().NotBeNull();
            value.Links.Self.Should().Be($"/appointments/{appointment.StringId}/relationships/reminders");
            value.Links.Related.Should().Be($"/appointments/{appointment.StringId}/reminders");
        });

        responseDocument.Included.Should().HaveCount(7);
        responseDocument.Included.Should().AllSatisfy(resource => resource.Type.Should().Be("reminders"));

        responseDocument.Meta.Should().ContainTotal(1);
    }

    [Fact]
    public async Task Can_get_all_secondary_resources()
    {
        // Arrange
        Appointment appointment = _fakers.Appointment.GenerateOne();
        appointment.Reminders = _fakers.Reminder.GenerateList(7);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Appointments.Add(appointment);
            await dbContext.SaveChangesAsync();
        });

        string route = $"appointments/{appointment.StringId}/reminders";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(7);
        responseDocument.Data.ManyValue.Should().AllSatisfy(resource => resource.Type.Should().Be("reminders"));

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.First.Should().BeNull();
        responseDocument.Links.Next.Should().BeNull();
        responseDocument.Links.Last.Should().BeNull();

        responseDocument.Meta.Should().ContainTotal(7);
    }

    [Fact]
    public async Task Can_get_ToMany_relationship()
    {
        // Arrange
        Appointment appointment = _fakers.Appointment.GenerateOne();
        appointment.Reminders = _fakers.Reminder.GenerateList(7);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Appointments.Add(appointment);
            await dbContext.SaveChangesAsync();
        });

        string route = $"appointments/{appointment.StringId}/relationships/reminders";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(7);
        responseDocument.Data.ManyValue.Should().AllSatisfy(resource => resource.Type.Should().Be("reminders"));

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.First.Should().BeNull();
        responseDocument.Links.Next.Should().BeNull();
        responseDocument.Links.Last.Should().BeNull();

        responseDocument.Meta.Should().ContainTotal(7);
    }

    [Fact]
    public async Task Ignores_pagination_from_query_string()
    {
        // Arrange
        Calendar calendar = _fakers.Calendar.GenerateOne();
        calendar.Appointments = _fakers.Appointment.GenerateSet(3);
        calendar.Appointments.ElementAt(0).Reminders = _fakers.Reminder.GenerateList(7);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Calendars.Add(calendar);
            await dbContext.SaveChangesAsync();
        });

        string route = $"calendars/{calendar.StringId}/appointments?include=reminders&page[size]=2,reminders:4";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);
        responseDocument.Data.ManyValue.Should().AllSatisfy(resource => resource.Type.Should().Be("appointments"));

        ResourceObject firstAppointment = responseDocument.Data.ManyValue.Single(resource => resource.Id == calendar.Appointments.ElementAt(0).StringId);

        firstAppointment.Relationships.Should().ContainKey("reminders").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();
            value.Data.ManyValue.Should().HaveCount(7);
        });

        responseDocument.Included.Should().HaveCount(7);
        responseDocument.Included.Should().AllSatisfy(resource => resource.Type.Should().Be("reminders"));

        responseDocument.Meta.Should().ContainTotal(3);
    }

    [Fact]
    public async Task Ignores_pagination_from_resource_definition()
    {
        // Arrange
        var paginationToggle = _testContext.Factory.Services.GetRequiredService<PaginationToggle>();
        paginationToggle.IsEnabled = true;

        Appointment appointment = _fakers.Appointment.GenerateOne();
        appointment.Reminders = _fakers.Reminder.GenerateList(7);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Appointments.Add(appointment);
            await dbContext.SaveChangesAsync();
        });

        string route = $"appointments/{appointment.StringId}/reminders";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(7);
        responseDocument.Data.ManyValue.Should().AllSatisfy(resource => resource.Type.Should().Be("reminders"));

        responseDocument.Meta.Should().ContainTotal(7);

        paginationToggle.IsCalled.Should().BeTrue();
    }

    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    private sealed class ReminderDefinition(PaginationToggle paginationToggle, IResourceGraph resourceGraph)
        : JsonApiResourceDefinition<Reminder, long>(resourceGraph)
    {
        private readonly PaginationToggle _paginationToggle = paginationToggle;

        public override PaginationExpression? OnApplyPagination(PaginationExpression? existingPagination)
        {
            _paginationToggle.IsCalled = true;
            return _paginationToggle.IsEnabled ? new PaginationExpression(PageNumber.ValueOne, new PageSize(4)) : existingPagination;
        }
    }

    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    private sealed class PaginationToggle
    {
        public bool IsEnabled { get; set; }
        public bool IsCalled { get; set; }
    }
}
