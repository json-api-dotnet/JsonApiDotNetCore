using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using FluentAssertions.Extensions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;

public sealed class SerializerIgnoreConditionTests : IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext>
{
    private readonly QueryStringFakers _fakers = new();

    public SerializerIgnoreConditionTests()
    {
        UseController<CalendarsController>();
    }

    [Theory]
    [InlineData(JsonIgnoreCondition.Never, true, true)]
    [InlineData(JsonIgnoreCondition.WhenWritingDefault, false, false)]
    [InlineData(JsonIgnoreCondition.WhenWritingNull, false, true)]
    public async Task Applies_configuration_for_ignore_condition(JsonIgnoreCondition configurationValue, bool expectNullValueInDocument,
        bool expectDefaultValueInDocument)
    {
        // Arrange
        var options = (JsonApiOptions)App.Services.GetRequiredService<IJsonApiOptions>();
        options.SerializerOptions.DefaultIgnoreCondition = configurationValue;

        Calendar calendar = _fakers.Calendar.GenerateOne();
        calendar.TimeZone = null;
        calendar.DefaultAppointmentDurationInMinutes = 0;
        calendar.ShowWeekNumbers = true;
        calendar.MostRecentAppointment = _fakers.Appointment.GenerateOne();
        calendar.MostRecentAppointment.Description = null;
        calendar.MostRecentAppointment.StartTime = default;
        calendar.MostRecentAppointment.EndTime = 1.January(2001).AsUtc();

        await RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Calendars.Add(calendar);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/calendars/{calendar.StringId}?include=mostRecentAppointment";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Included.Should().HaveCount(1);

        if (expectNullValueInDocument)
        {
            responseDocument.Data.SingleValue.Attributes.Should().ContainKey("timeZone");
            responseDocument.Included[0].Attributes.Should().ContainKey("description");
        }
        else
        {
            responseDocument.Data.SingleValue.Attributes.Should().NotContainKey("timeZone");
            responseDocument.Included[0].Attributes.Should().NotContainKey("description");
        }

        if (expectDefaultValueInDocument)
        {
            responseDocument.Data.SingleValue.Attributes.Should().ContainKey("defaultAppointmentDurationInMinutes");
            responseDocument.Included[0].Attributes.Should().ContainKey("startTime");
        }
        else
        {
            responseDocument.Data.SingleValue.Attributes.Should().NotContainKey("defaultAppointmentDurationInMinutes");
            responseDocument.Included[0].Attributes.Should().NotContainKey("startTime");
        }
    }

    [Theory]
    [InlineData(JsonIgnoreCondition.Never, true, true)]
    [InlineData(JsonIgnoreCondition.WhenWritingDefault, false, false)]
    [InlineData(JsonIgnoreCondition.WhenWritingNull, false, true)]
    public async Task Applies_ignore_condition_to_nested_members(JsonIgnoreCondition configurationValue, bool expectNullValueInDocument,
        bool expectDefaultValueInDocument)
    {
        // Arrange
        var options = (JsonApiOptions)App.Services.GetRequiredService<IJsonApiOptions>();
        options.SerializerOptions.DefaultIgnoreCondition = configurationValue;

        Calendar calendar = _fakers.Calendar.GenerateOne();
        calendar.MostRecentAppointment = _fakers.Appointment.GenerateOne();

        calendar.MostRecentAppointment.Cost = new AppointmentCost
        {
            Amount = 0,
            Currency = null
        };

        calendar.MostRecentAppointment.Location = new AppointmentLocation
        {
            Name = null,
            Room = "R1",
            Address = new AppointmentAddress
            {
                City = null,
                CountryCode = "NL"
            }
        };

        await RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Calendars.Add(calendar);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/calendars/{calendar.StringId}?include=mostRecentAppointment";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Included.Should().HaveCount(1);
        ResourceObject included = responseDocument.Included[0];

        JsonElement cost = included.GetAttribute("cost");
        cost.TryGetProperty("currency", out _).Should().Be(expectNullValueInDocument);
        cost.TryGetProperty("amount", out _).Should().Be(expectDefaultValueInDocument);

        JsonElement location = included.GetAttribute("location");
        location.TryGetProperty("name", out _).Should().Be(expectNullValueInDocument);
        location.TryGetProperty("room", out _).Should().BeTrue();

        JsonElement address = location.GetProperty("address");
        address.TryGetProperty("city", out _).Should().Be(expectNullValueInDocument);
        address.TryGetProperty("countryCode", out _).Should().BeTrue();
    }

    [Theory]
    [InlineData(JsonIgnoreCondition.Never, true)]
    [InlineData(JsonIgnoreCondition.WhenWritingDefault, false)]
    [InlineData(JsonIgnoreCondition.WhenWritingNull, false)]
    public async Task Applies_ignore_condition_to_top_level_compound_attribute(JsonIgnoreCondition configurationValue, bool expectNullValueInDocument)
    {
        // Arrange
        var options = (JsonApiOptions)App.Services.GetRequiredService<IJsonApiOptions>();
        options.SerializerOptions.DefaultIgnoreCondition = configurationValue;

        Calendar calendar = _fakers.Calendar.GenerateOne();
        calendar.Workweek = null;
        calendar.Audit = null;

        await RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Calendars.Add(calendar);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/calendars/{calendar.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();

        if (expectNullValueInDocument)
        {
            responseDocument.Data.SingleValue.GetAttribute("workweek").ValueKind.Should().Be(JsonValueKind.Null);
            responseDocument.Data.SingleValue.GetAttribute("audit").ValueKind.Should().Be(JsonValueKind.Null);
        }
        else
        {
            responseDocument.Data.SingleValue.Attributes.Should().NotContainKey("workweek");
            responseDocument.Data.SingleValue.Attributes.Should().NotContainKey("audit");
        }
    }

    [Fact(Skip = "EF Core 10: Null elements in a complex collection cannot be materialized ('Invalid token type: Null').")]
    public async Task Keeps_null_elements_in_compound_collection()
    {
        // Arrange
        var options = (JsonApiOptions)App.Services.GetRequiredService<IJsonApiOptions>();
        options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

        Calendar calendar = _fakers.Calendar.GenerateOne();

        calendar.Holidays =
        [
            new Holiday
            {
                Name = "New Year"
            },
            null!
        ];

        await RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Calendars.Add(calendar);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/calendars/{calendar.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        JsonElement holidays = responseDocument.Data.SingleValue.GetAttribute("holidays");
        holidays.GetArrayLength().Should().Be(2);
        holidays[1].ValueKind.Should().Be(JsonValueKind.Null);
    }
}
