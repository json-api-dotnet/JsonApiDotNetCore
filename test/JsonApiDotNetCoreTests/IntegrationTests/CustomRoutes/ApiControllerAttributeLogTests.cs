using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.CustomRoutes;

public sealed class ApiControllerAttributeLogTests : IntegrationTestContext<TestableStartup<CustomRouteDbContext>, CustomRouteDbContext>
{
    private readonly FakeLoggerFactory _loggerFactory;

    public ApiControllerAttributeLogTests()
    {
        UseController<CiviliansController>();

        _loggerFactory = new FakeLoggerFactory(LogLevel.Warning);

        ConfigureLogging(options =>
        {
            options.ClearProviders();
            options.AddProvider(_loggerFactory);
        });

        ConfigureServices(services =>
        {
            services.AddSingleton(_loggerFactory);
        });
    }

    [Fact]
    public void Logs_warning_at_startup_when_ApiControllerAttribute_found()
    {
        // Arrange
        _loggerFactory.Logger.Clear();

        // Act
        _ = Factory;

        // Assert
        IReadOnlyList<string> logLines = _loggerFactory.Logger.GetLines();
        logLines.ShouldHaveCount(1);

        logLines[0].Should().Be(
            $"[WARNING] Found JSON:API controller '{typeof(CiviliansController)}' with [ApiController]. Please remove this attribute for optimal JSON:API compliance.");
    }
}
