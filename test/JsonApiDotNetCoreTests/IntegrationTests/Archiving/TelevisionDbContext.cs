using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.Archiving;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class TelevisionDbContext(DbContextOptions<TelevisionDbContext> options)
    : TestableDbContext(options)
{
    public DbSet<TelevisionNetwork> Networks => Set<TelevisionNetwork>();
    public DbSet<TelevisionStation> Stations => Set<TelevisionStation>();
    public DbSet<TelevisionBroadcast> Broadcasts => Set<TelevisionBroadcast>();
    public DbSet<BroadcastComment> Comments => Set<BroadcastComment>();
}
