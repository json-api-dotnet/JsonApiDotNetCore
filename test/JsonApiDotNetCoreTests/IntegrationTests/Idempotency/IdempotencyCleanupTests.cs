using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.Idempotency;

public sealed class IdempotencyCleanupTests : IClassFixture<IntegrationTestContext<TestableStartup<IdempotencyDbContext>, IdempotencyDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<IdempotencyDbContext>, IdempotencyDbContext> _testContext;

    public IdempotencyCleanupTests(IntegrationTestContext<TestableStartup<IdempotencyDbContext>, IdempotencyDbContext> testContext)
    {
        _testContext = testContext;

        _testContext.ConfigureServicesAfterStartup(services =>
        {
            services.AddDbContextFactory<IdempotencyDbContext>();

            services.AddScoped<ISystemClock, FrozenSystemClock>();
            services.AddSingleton<IdempotencyCleanupJob>();
        });
    }

    [Fact]
    public async Task Removes_expired_items()
    {
        // Arrange
        var clock = (FrozenSystemClock)_testContext.Factory.Services.GetRequiredService<ISystemClock>();
        clock.UtcNow = 26.March(2005).At(12, 13, 14, 15, 16).AsUtc();

        var existingItems = new List<RequestCacheItem>
        {
            new("A", "", 1.January(1960).AsUtc()),
            new("B", "", 1.January(2005).AsUtc()),
            new("C", "", 1.January(2009).AsUtc())
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<RequestCacheItem>();
            dbContext.RequestCache.AddRange(existingItems);
            await dbContext.SaveChangesAsync();
        });

        var job = _testContext.Factory.Services.GetRequiredService<IdempotencyCleanupJob>();

        // Act
        await job.RunOnceAsync(CancellationToken.None);

        // Assert
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            List<RequestCacheItem> itemsInDatabase = await dbContext.RequestCache.ToListAsync();

            itemsInDatabase.ShouldHaveCount(1);
            itemsInDatabase[0].Id.Should().Be("C");
        });
    }
}
