using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCoreExampleTests.Startups;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Logging
{
    public sealed class LoggingTests
        : IClassFixture<ExampleIntegrationTestContext<TestableStartup<AuditDbContext>, AuditDbContext>>
    {
        private readonly ExampleIntegrationTestContext<TestableStartup<AuditDbContext>, AuditDbContext> _testContext;
        private readonly AuditFakers _fakers = new AuditFakers();

        public LoggingTests(ExampleIntegrationTestContext<TestableStartup<AuditDbContext>, AuditDbContext> testContext)
        {
            _testContext = testContext;

            FakeLoggerFactory loggerFactory = null;

            testContext.ConfigureLogging(options =>
            {
                loggerFactory = new FakeLoggerFactory();

                options.ClearProviders();
                options.AddProvider(loggerFactory);
                options.SetMinimumLevel(LogLevel.Trace);
                options.AddFilter((category, level) => true);
            });

            testContext.ConfigureServicesBeforeStartup(services =>
            {
                if (loggerFactory != null)
                {
                    services.AddSingleton(_ => loggerFactory);
                }
            });
        }

        [Fact]
        public async Task Logs_request_body_at_Trace_level()
        {
            // Arrange
            var loggerFactory = _testContext.Factory.Services.GetRequiredService<FakeLoggerFactory>();
            loggerFactory.Logger.Clear();

            var newEntry = _fakers.AuditEntry.Generate();

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
            var route = "/auditEntries";

            // Act
            var (httpResponse, _) = await _testContext.ExecutePostAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            loggerFactory.Logger.Messages.Should().NotBeEmpty();

            loggerFactory.Logger.Messages.Should().ContainSingle(message => message.LogLevel == LogLevel.Trace &&
                message.Text.StartsWith("Received request at 'http://localhost/auditEntries' with body: <<"));
        }

        [Fact]
        public async Task Logs_response_body_at_Trace_level()
        {
            // Arrange
            var loggerFactory = _testContext.Factory.Services.GetRequiredService<FakeLoggerFactory>();
            loggerFactory.Logger.Clear();

            // Arrange
            var route = "/auditEntries";

            // Act
            var (httpResponse, _) = await _testContext.ExecuteGetAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            loggerFactory.Logger.Messages.Should().NotBeEmpty();

            loggerFactory.Logger.Messages.Should().ContainSingle(message => message.LogLevel == LogLevel.Trace && 
                message.Text.StartsWith("Sending 200 response for request at 'http://localhost/auditEntries' with body: <<"));
        }

        [Fact]
        public async Task Logs_invalid_request_body_error_at_Information_level()
        {
            // Arrange
            var loggerFactory = _testContext.Factory.Services.GetRequiredService<FakeLoggerFactory>();
            loggerFactory.Logger.Clear();

            // Arrange
            var requestBody = "{ \"data\" {";

            var route = "/auditEntries";

            // Act
            var (httpResponse, _) = await _testContext.ExecutePostAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            loggerFactory.Logger.Messages.Should().NotBeEmpty();

            loggerFactory.Logger.Messages.Should().ContainSingle(message => message.LogLevel == LogLevel.Information &&
                message.Text.Contains("Failed to deserialize request body."));
        }
    }
}
