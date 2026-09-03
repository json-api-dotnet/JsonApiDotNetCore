using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.Endpoints.JsonApiControllers.ApiControllerAnnotation;

public sealed class ApiControllerAnnotationLogTests
    : IntegrationTestContext<TestableStartup<ApiControllerAnnotationDbContext>, ApiControllerAnnotationDbContext>, IAsyncDisposable
{
    private readonly CapturingLoggerProvider _loggerProvider;

    public ApiControllerAnnotationLogTests()
    {
        UseController<LoginTokensController>();

        _loggerProvider = new CapturingLoggerProvider(LogLevel.Warning);

        ConfigureLogging(builder =>
        {
            builder.AddProvider(_loggerProvider);

            builder.Services.AddSingleton(_loggerProvider);
        });
    }

    [Fact]
    public void Logs_warning_at_startup_when_ApiControllerAttribute_found()
    {
        // Arrange
        _loggerProvider.Clear();

        // Act
        _ = Factory;

        // Assert
        IReadOnlyList<string> logLines = _loggerProvider.GetLines();
        logLines.Should().HaveCount(1);

        logLines[0].Should().Be($"[WARNING] Found JSON:API controller '{typeof(LoginTokensController)}' with [ApiController]. " +
            $"Please remove this attribute for optimal JSON:API compliance.");
    }

    public override Task DisposeAsync()
    {
        _loggerProvider.Dispose();
        return base.DisposeAsync();
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        await DisposeAsync();
    }
}
