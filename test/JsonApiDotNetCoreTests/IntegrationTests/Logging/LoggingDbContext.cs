using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.Logging;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class LoggingDbContext : TestableDbContext
{
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
    public DbSet<FruitBowl> FruitBowls => Set<FruitBowl>();
    public DbSet<Fruit> Fruits => Set<Fruit>();
    public DbSet<Banana> Bananas => Set<Banana>();
    public DbSet<Peach> Peaches => Set<Peach>();

    public LoggingDbContext(DbContextOptions<LoggingDbContext> options)
        : base(options)
    {
    }
}
