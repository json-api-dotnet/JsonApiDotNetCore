using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.Filtering;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class FilterDbContext(DbContextOptions<FilterDbContext> options)
    : TestableDbContext(options)
{
    public DbSet<FilterableResource> FilterableResources => Set<FilterableResource>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<FilterableResource>()
            .Property(resource => resource.SomeDateTimeInLocalZone)
            .HasColumnType("timestamp without time zone");

        builder.Entity<FilterableResource>()
            .ComplexProperty(resource => resource.SomeCompound, complexBuilder => complexBuilder.ToJson());

        builder.Entity<FilterableResource>()
            .ComplexProperty(resource => resource.SomeRequiredCompound, complexBuilder => complexBuilder.ToJson());

        builder.Entity<FilterableResource>()
            .ComplexCollection(resource => resource.SomeCompounds, complexBuilder => complexBuilder.ToJson());

        builder.Entity<FilterableResource>()
            .ComplexCollection(resource => resource.SomeLabeledDates, complexBuilder => complexBuilder.ToJson());

        base.OnModelCreating(builder);
    }
}
