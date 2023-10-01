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

        ConfigureServicesBeforeStartup(services =>
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
        _loggerFactory.Logger.Messages.ShouldHaveCount(1);
        _loggerFactory.Logger.Messages.Single().LogLevel.Should().Be(LogLevel.Warning);

        _loggerFactory.Logger.Messages.Single().Text.Should().Be(
            $"Found JSON:API controller '{typeof(CiviliansController)}' with [ApiController]. Please remove this attribute for optimal JSON:API compliance.");
    }
}
