using System.Net;
using FluentAssertions;
using Humanizer;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.TimeOffset;

public sealed class TimeOffsetTests : IClassFixture<IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;
    private readonly QueryStringFakers _fakers = new();

    public TimeOffsetTests(IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<CalendarsController>();
        testContext.UseController<RemindersController>();

        testContext.ConfigureServices(services =>
        {
            services.AddTransient<IFilterParser, TimeOffsetFilterParser>();
            services.AddScoped(typeof(IResourceDefinition<,>), typeof(FilterRewritingResourceDefinition<,>));
        });
    }

    [Theory]
    [InlineData("-0:10:00", ComparisonOperator.GreaterThan, "0")] // more than 10 minutes ago
    [InlineData("-0:10:00", ComparisonOperator.GreaterOrEqual, "0,1")] // at least 10 minutes ago
    [InlineData("-0:10:00", ComparisonOperator.Equals, "1")] // exactly 10 minutes ago
    [InlineData("-0:10:00", ComparisonOperator.LessThan, "2,3")] // less than 10 minutes ago
    [InlineData("-0:10:00", ComparisonOperator.LessOrEqual, "1,2,3")] // at most 10 minutes ago
    [InlineData("+0:10:00", ComparisonOperator.GreaterThan, "6")] // more than 10 minutes in the future
    [InlineData("+0:10:00", ComparisonOperator.GreaterOrEqual, "5,6")] // at least 10 minutes in the future
    [InlineData("+0:10:00", ComparisonOperator.Equals, "5")] // in exactly 10 minutes
    [InlineData("+0:10:00", ComparisonOperator.LessThan, "3,4")] // less than 10 minutes in the future
    [InlineData("+0:10:00", ComparisonOperator.LessOrEqual, "3,4,5")] // at most 10 minutes in the future
    public async Task Can_filter_comparison_on_relative_time(string filterValue, ComparisonOperator comparisonOperator, string matchingRowsExpected)
    {
        // Arrange
        var timeProvider = _testContext.Factory.Services.GetRequiredService<TimeProvider>();
        DateTimeOffset utcNow = timeProvider.GetUtcNow();

        List<Reminder> reminders = _fakers.Reminder.GenerateList(7);
        reminders[0].RemindsAt = utcNow.Add(TimeSpan.FromMinutes(-15)).UtcDateTime;
        reminders[1].RemindsAt = utcNow.Add(TimeSpan.FromMinutes(-10)).UtcDateTime;
        reminders[2].RemindsAt = utcNow.Add(TimeSpan.FromMinutes(-5)).UtcDateTime;
        reminders[3].RemindsAt = utcNow.Add(TimeSpan.FromMinutes(0)).UtcDateTime;
        reminders[4].RemindsAt = utcNow.Add(TimeSpan.FromMinutes(5)).UtcDateTime;
        reminders[5].RemindsAt = utcNow.Add(TimeSpan.FromMinutes(10)).UtcDateTime;
        reminders[6].RemindsAt = utcNow.Add(TimeSpan.FromMinutes(15)).UtcDateTime;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Reminder>();
            dbContext.Reminders.AddRange(reminders);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/reminders?filter={comparisonOperator.ToString().Camelize()}(remindsAt,timeOffset('{filterValue.Replace("+", "%2B")}'))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        int[] matchingRowIndices = matchingRowsExpected.Split(',').Select(int.Parse).ToArray();
        responseDocument.Data.ManyValue.Should().HaveCount(matchingRowIndices.Length);

        foreach (int rowIndex in matchingRowIndices)
        {
            responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Id == reminders[rowIndex].StringId);
        }
    }

    [Fact]
    public async Task Cannot_filter_comparison_on_missing_relative_time()
    {
        // Arrange
        var parameterValue = new MarkedText("equals(remindsAt,timeOffset(^", '^');
        string route = $"/reminders?filter={parameterValue.Text}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified filter is invalid.");
        error.Detail.Should().Be($"Time offset between quotes expected. {parameterValue}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("filter");
    }

    [Fact]
    public async Task Cannot_filter_comparison_on_invalid_relative_time()
    {
        // Arrange
        var parameterValue = new MarkedText("equals(remindsAt,timeOffset(^'-*'))", '^');
        string route = $"/reminders?filter={parameterValue.Text}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified filter is invalid.");
        error.Detail.Should().Be($"Failed to convert '*' of type 'String' to type 'TimeSpan'. {parameterValue}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("filter");
    }

    [Fact]
    public async Task Cannot_filter_comparison_on_relative_time_at_left_side()
    {
        // Arrange
        var parameterValue = new MarkedText("^equals(timeOffset('-0:10:00'),remindsAt)", '^');
        string route = $"/reminders?filter={parameterValue.Text}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified filter is invalid.");
        error.Detail.Should().Be($"The 'timeOffset' function can only be used at the right side of comparisons. {parameterValue}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("filter");
    }

    [Fact]
    public async Task Cannot_filter_any_on_relative_time()
    {
        // Arrange
        var parameterValue = new MarkedText("any(remindsAt,^timeOffset('-0:10:00'))", '^');
        string route = $"/reminders?filter={parameterValue.Text}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified filter is invalid.");
        error.Detail.Should().Be($"Value between quotes expected. {parameterValue}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("filter");
    }

    [Fact]
    public async Task Cannot_filter_text_match_on_relative_time()
    {
        // Arrange
        var parameterValue = new MarkedText("startsWith(^remindsAt,timeOffset('-0:10:00'))", '^');
        string route = $"/reminders?filter={parameterValue.Text}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified filter is invalid.");
        error.Detail.Should().Be($"Attribute of type 'String' expected. {parameterValue}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("filter");
    }

    [Fact]
    public async Task Can_filter_comparison_on_relative_time_in_nested_expression()
    {
        // Arrange
        var timeProvider = _testContext.Factory.Services.GetRequiredService<TimeProvider>();
        DateTimeOffset utcNow = timeProvider.GetUtcNow();

        Calendar calendar = _fakers.Calendar.GenerateOne();
        calendar.Appointments = _fakers.Appointment.GenerateSet(2);

        calendar.Appointments.ElementAt(0).Reminders = _fakers.Reminder.GenerateList(1);
        calendar.Appointments.ElementAt(0).Reminders[0].RemindsAt = utcNow.UtcDateTime;

        calendar.Appointments.ElementAt(1).Reminders = _fakers.Reminder.GenerateList(1);
        calendar.Appointments.ElementAt(1).Reminders[0].RemindsAt = utcNow.Add(TimeSpan.FromMinutes(30)).UtcDateTime;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Calendars.Add(calendar);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/calendars/{calendar.StringId}/appointments?filter=has(reminders,equals(remindsAt,timeOffset('%2B0:30:00')))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);

        responseDocument.Data.ManyValue[0].Id.Should().Be(calendar.Appointments.ElementAt(1).StringId);
    }
}
