using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreTests.IntegrationTests.Archiving;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class TelevisionDbContext(DbContextOptions<TelevisionDbContext> options)
    : TestableDbContext(options)
{
    public DbSet<TelevisionNetwork> Networks => Set<TelevisionNetwork>();
    public DbSet<TelevisionStation> Stations => Set<TelevisionStation>();
    public DbSet<TelevisionBroadcast> Broadcasts => Set<TelevisionBroadcast>();
    public DbSet<BroadcastComment> Comments => Set<BroadcastComment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<TelevisionNetwork>()
            .ComplexProperty(network => network.Profile, complexBuilder => complexBuilder.ToJson());

        base.OnModelCreating(builder);
    }
}
