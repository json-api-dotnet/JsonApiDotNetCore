using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.BackgroundProcessing;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class BackgroundJobDbContext : TestableDbContext
{
    public DbSet<WorkItem> WorkItems => Set<WorkItem>();

    public BackgroundJobDbContext(DbContextOptions<BackgroundJobDbContext> options)
        : base(options)
    {
    }
}