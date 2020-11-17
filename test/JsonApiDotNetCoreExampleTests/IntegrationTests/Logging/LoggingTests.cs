using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Logging
{
    public sealed class LoggingTests : IClassFixture<IntegrationTestContext<Startup, AppDbContext>>
    {
        private readonly IntegrationTestContext<Startup, AppDbContext> _testContext;

        public LoggingTests(IntegrationTestContext<Startup, AppDbContext> testContext)
        {
            _testContext = testContext;

            FakeLoggerFactory loggerFactory = null;

            testContext.ConfigureLogging(options =>
            {
                loggerFactory = new FakeLoggerFactory();

                options.ClearProviders();
                options.AddProvider(loggerFactory);
                options.SetMinimumLevel(LogLevel.Trace);
                options.AddFilter((category, level) => level == LogLevel.Trace && 
                    (category == typeof(JsonApiReader).FullName || category == typeof(JsonApiWriter).FullName));
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
        public async Task Logs_request_body_on_error()
        {
            // Arrange
            var loggerFactory = _testContext.Factory.Services.GetRequiredService<FakeLoggerFactory>();
            loggerFactory.Logger.Clear();

            // Arrange
            var requestBody = "{ \"data\" {";

            var route = "/api/v1/todoItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);

            loggerFactory.Logger.Messages.Should().HaveCount(2);
            loggerFactory.Logger.Messages.Should().Contain(message => message.Text.StartsWith("Received request at ") && message.Text.Contains("with body:"));
            loggerFactory.Logger.Messages.Should().Contain(message => message.Text.StartsWith("Sending 422 response for request at ") && message.Text.Contains("Failed to deserialize request body."));
        }
    }
}
