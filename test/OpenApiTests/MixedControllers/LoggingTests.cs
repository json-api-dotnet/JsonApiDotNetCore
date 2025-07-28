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

    [Fact]
    public async Task Logs_warning_for_unsupported_custom_actions_in_JsonApi_controllers()
    {
        // Arrange
        var loggerProvider = _testContext.Factory.Services.GetRequiredService<CapturingLoggerProvider>();

        // Act
        await _testContext.GetSwaggerDocumentAsync();

        // Assert
        IReadOnlyList<string> logLines = loggerProvider.GetLines();

        logLines.Should().BeEquivalentTo(new[]
        {
            $"[WARNING] Hiding unsupported custom JSON:API action method [GET] {typeof(CoffeeSummaryController)}.GetSummaryAsync (OpenApiTests) in OpenAPI.",
            $"[WARNING] Hiding unsupported custom JSON:API action method [HEAD] {typeof(CoffeeSummaryController)}.GetSummaryAsync (OpenApiTests) in OpenAPI.",
            $"[WARNING] Hiding unsupported custom JSON:API action method [DELETE] {typeof(CoffeeSummaryController)}.DeleteOnlyMilkAsync (OpenApiTests) in OpenAPI."
        }, options => options.WithStrictOrdering());
    }
}
