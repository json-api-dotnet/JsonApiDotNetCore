using JetBrains.Annotations;
using JsonApiDotNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace JsonApiDotNetCoreTests.IntegrationTests.Idempotency;

internal sealed class IdempotencyCleanupJob
{
    private static readonly TimeSpan CacheExpirationTime = TimeSpan.FromDays(31);
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(1);

    private readonly ISystemClock _systemClock;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly IDbContextFactory<IdempotencyDbContext> _dbContextFactory;

    public IdempotencyCleanupJob(ISystemClock systemClock, IHostApplicationLifetime hostApplicationLifetime,
        IDbContextFactory<IdempotencyDbContext> dbContextFactory)
    {
        ArgumentGuard.NotNull(systemClock, nameof(systemClock));
        ArgumentGuard.NotNull(hostApplicationLifetime, nameof(hostApplicationLifetime));
        ArgumentGuard.NotNull(dbContextFactory, nameof(dbContextFactory));

        _systemClock = systemClock;
        _hostApplicationLifetime = hostApplicationLifetime;
        _dbContextFactory = dbContextFactory;
    }

    /// <summary>
    /// Schedule this method to run on a pooled background thread from Program.cs, using the code below. See also:
    /// https://stackoverflow.com/questions/26921191/how-to-pass-longrunning-flag-specifically-to-task-run
    /// <example>
    /// <code><![CDATA[
    /// builder.Services.AddSingleton<IdempotencyCleanupJob>();
    /// 
    /// WebApplication app = builder.Build();
    /// 
    /// var job = app.Services.GetRequiredService<IdempotencyCleanupJob>();
    /// 
    /// _ = Task.Run(async () =>
    /// {
    ///     await job.StartPeriodicPurgeOfExpiredItemsAsync();
    /// });
    /// 
    /// app.Run();
    /// ]]></code>
    /// </example>
    /// </summary>
    [PublicAPI]
    public async Task StartPeriodicPurgeOfExpiredItemsAsync()
    {
        await StartPeriodicPurgeOfExpiredItemsAsync(_hostApplicationLifetime.ApplicationStopping);
    }

    private async Task StartPeriodicPurgeOfExpiredItemsAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(CleanupInterval);

        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                await RunIterationAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    public async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        await RunIterationAsync(cancellationToken);
    }

    private async Task RunIterationAsync(CancellationToken cancellationToken)
    {
        DateTimeOffset threshold = _systemClock.UtcNow - CacheExpirationTime;

        await using IdempotencyDbContext dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        List<RequestCacheItem> itemsToDelete = await dbContext.RequestCache.Where(item => item.CreatedAt < threshold).ToListAsync(cancellationToken);

        dbContext.RemoveRange(itemsToDelete);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
