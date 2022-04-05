using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.Logging;

public sealed class LoggingTests : IClassFixture<IntegrationTestContext<TestableStartup<LoggingDbContext>, LoggingDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<LoggingDbContext>, LoggingDbContext> _testContext;
    private readonly LoggingFakers _fakers = new();

    public LoggingTests(IntegrationTestContext<TestableStartup<LoggingDbContext>, LoggingDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<AuditEntriesController>();

        var loggerFactory = new FakeLoggerFactory(LogLevel.Trace);

        testContext.ConfigureLogging(options =>
        {
            options.ClearProviders();
            options.AddProvider(loggerFactory);
            options.SetMinimumLevel(LogLevel.Trace);
        });

        testContext.ConfigureServicesBeforeStartup(services =>
        {
            services.AddSingleton(loggerFactory);
        });
    }

    [Fact]
    public async Task Logs_request_body_at_Trace_level()
    {
        // Arrange
        var loggerFactory = _testContext.Factory.Services.GetRequiredService<FakeLoggerFactory>();
        loggerFactory.Logger.Clear();

        AuditEntry newEntry = _fakers.AuditEntry.Generate();

        var requestBody = new
        {
            data = new
            {
                type = "auditEntries",
                attributes = new
                {
                    userName = newEntry.UserName,
                    createdAt = newEntry.CreatedAt
                }
            }
        };

        // Arrange
        const string route = "/auditEntries";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        loggerFactory.Logger.Messages.ShouldNotBeEmpty();

        loggerFactory.Logger.Messages.Should().ContainSingle(message => message.LogLevel == LogLevel.Trace &&
            message.Text.StartsWith("Received POST request at 'http://localhost/auditEntries' with body: <<", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Logs_response_body_at_Trace_level()
    {
        // Arrange
        var loggerFactory = _testContext.Factory.Services.GetRequiredService<FakeLoggerFactory>();
        loggerFactory.Logger.Clear();

        // Arrange
        const string route = "/auditEntries";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        loggerFactory.Logger.Messages.ShouldNotBeEmpty();

        loggerFactory.Logger.Messages.Should().ContainSingle(message => message.LogLevel == LogLevel.Trace &&
            message.Text.StartsWith("Sending 200 response for GET request at 'http://localhost/auditEntries' with body: <<", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Logs_invalid_request_body_error_at_Information_level()
    {
        // Arrange
        var loggerFactory = _testContext.Factory.Services.GetRequiredService<FakeLoggerFactory>();
        loggerFactory.Logger.Clear();

        // Arrange
        const string requestBody = "{ \"data\" {";

        const string route = "/auditEntries";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        loggerFactory.Logger.Messages.ShouldNotBeEmpty();

        loggerFactory.Logger.Messages.Should().ContainSingle(message => message.LogLevel == LogLevel.Information &&
            message.Text.Contains("Failed to deserialize request body."));
    }
}
