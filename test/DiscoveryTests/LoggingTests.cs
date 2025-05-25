using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestBuildingBlocks;
using Xunit;

namespace DiscoveryTests;

public sealed class LoggingTests
{
    [Fact]
    public async Task Logs_message_to_add_NuGet_reference()
    {
        // Arrange
        using var loggerProvider =
            new CapturingLoggerProvider((category, _) => category.StartsWith("JsonApiDotNetCore.Repositories", StringComparison.Ordinal));

        WebApplicationBuilder builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions());
        builder.Logging.AddProvider(loggerProvider);
        builder.Logging.SetMinimumLevel(LogLevel.Debug);
        builder.Services.AddDbContext<TestDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        builder.Services.AddJsonApi<TestDbContext>();
        builder.WebHost.UseTestServer();
        await using WebApplication app = builder.Build();

        var resourceGraph = app.Services.GetRequiredService<IResourceGraph>();
        ResourceType resourceType = resourceGraph.GetResourceType<PrivateResource>();

        var repository = app.Services.GetRequiredService<IResourceRepository<PrivateResource, int>>();

        // Act
        _ = await repository.GetAsync(new QueryLayer(resourceType), CancellationToken.None);

        // Assert
        IReadOnlyList<string> logLines = loggerProvider.GetLines();

        logLines.Should().Contain(
            "[DEBUG] Failed to load assembly. To log expression trees, add a NuGet reference to 'AgileObjects.ReadableExpressions' in your project.");
    }

    private sealed class TestDbContext(DbContextOptions options)
        : DbContext(options)
    {
        public DbSet<PrivateResource> PrivateResources => Set<PrivateResource>();
    }
}
