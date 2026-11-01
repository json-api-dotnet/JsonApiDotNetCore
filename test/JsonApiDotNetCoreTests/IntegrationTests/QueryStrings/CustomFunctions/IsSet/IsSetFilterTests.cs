using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.Queries.QueryableBuilding;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.IsSet;

public sealed class IsSetFilterTests : IClassFixture<IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;
    private readonly QueryStringFakers _fakers = new();

    public IsSetFilterTests(IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<AppointmentsController>();

        testContext.ConfigureServices(services =>
        {
            services.AddTransient<IFilterParser, IsSetFilterParser>();
            services.AddTransient<IWhereClauseBuilder, IsSetWhereClauseBuilder>();
        });
    }

    [Theory]
    [InlineData("isSet(description)")]
    [InlineData("isSet(location)")]
    [InlineData("isSet(location.address)")]
    [InlineData("isSet(location.address.city)")]
    [InlineData("isSet(keywords)")]
    public async Task Can_filter_on_attribute_of_any_kind(string filterExpression)
    {
        // Arrange
        List<Appointment> appointments = _fakers.Appointment.GenerateList(2);

        appointments[0].Description = null;
        appointments[0].Location = null;
        appointments[0].Keywords = null;

        appointments[1].Description = "Some text";
        appointments[1].Keywords = ["a"];

        appointments[1].Location = new AppointmentLocation
        {
            Address = new AppointmentAddress
            {
                City = "Rotterdam"
            }
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Appointment>();
            dbContext.Appointments.AddRange(appointments);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/appointments?filter={filterExpression}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Type.Should().Be("appointments");
        responseDocument.Data.ManyValue[0].Id.Should().Be(appointments[1].StringId);
    }

    [Theory]
    [InlineData("has(attendees,isSet(email))")]
    [InlineData("has(attendees,isSet(contact))")]
    [InlineData("has(attendees,isSet(contact.title))")]
    public async Task Can_filter_on_attribute_in_compound_collection_scope(string filterExpression)
    {
        // Arrange
        List<Appointment> appointments = _fakers.Appointment.GenerateList(2);

        appointments[0].Attendees =
        [
            new Attendee
            {
                Name = "Alice",
                Email = null,
                Contact = null
            }
        ];

        appointments[1].Attendees =
        [
            new Attendee
            {
                Name = "Bob",
                Email = "bob@example.com",
                Contact = new AttendeeContact
                {
                    Title = "Dr"
                }
            }
        ];

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Appointment>();
            dbContext.Appointments.AddRange(appointments);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/appointments?filter={filterExpression}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(appointments[1].StringId);
    }

    [Fact]
    public async Task Cannot_filter_on_to_many_relationship_using_attribute_pattern()
    {
        // Arrange
        const string route = "/appointments?filter=isSet(reminders)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified filter is invalid.");
        error.Detail.Should().StartWith("Field chain on resource type 'appointments' failed to match the pattern: ");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("filter");
    }
}
