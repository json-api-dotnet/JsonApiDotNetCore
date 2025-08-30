using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiTests.MixedControllers;

public sealed class LoggingTests : IClassFixture<OpenApiTestContext<MixedControllerStartup, CoffeeDbContext>>
{
    private readonly OpenApiTestContext<MixedControllerStartup, CoffeeDbContext> _testContext;

    public LoggingTests(OpenApiTestContext<MixedControllerStartup, CoffeeDbContext> testContext, ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;

        testContext.UseController<CoffeeSummaryController>();

        testContext.SetTestOutputHelper(testOutputHelper);

        testContext.ConfigureServices(services => services.AddLogging(builder =>
        {
            var loggerProvider = new CapturingLoggerProvider(LogLevel.Warning);
            builder.AddProvider(loggerProvider);
            builder.SetMinimumLevel(LogLevel.Warning);

            builder.Services.AddSingleton(loggerProvider);
        }));
    }
}
