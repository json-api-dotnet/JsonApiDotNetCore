using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.Filtering;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class FilterDbContext(DbContextOptions<FilterDbContext> options) : TestableDbContext(options)
{
    public DbSet<FilterableResource> FilterableResources => Set<FilterableResource>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<FilterableResource>()
            .Property(resource => resource.SomeDateTimeInLocalZone)
            .HasColumnType("timestamp without time zone");

        base.OnModelCreating(builder);
    }
}
