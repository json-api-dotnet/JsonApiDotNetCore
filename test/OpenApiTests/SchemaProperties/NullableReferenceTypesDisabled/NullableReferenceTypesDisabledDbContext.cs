using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using OpenApiTests.SchemaProperties.NullableReferenceTypesDisabled.RelationshipsObject;
using TestBuildingBlocks;

namespace OpenApiTests.SchemaProperties.NullableReferenceTypesDisabled;

// @formatter:wrap_chained_method_calls chop_always

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class NullableReferenceTypesDisabledDbContext : TestableDbContext
{
    public DbSet<Chicken> Chicken => Set<Chicken>();
    public DbSet<HenHouse> HenHouse => Set<HenHouse>();

    public NullableReferenceTypesDisabledDbContext(DbContextOptions<NullableReferenceTypesDisabledDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<HenHouse>()
            .HasOne(resource => resource.OldestChicken);

        builder.Entity<HenHouse>()
            .HasOne(resource => resource.FirstChicken);

        builder.Entity<HenHouse>()
            .HasMany(resource => resource.AllChickens);

        builder.Entity<HenHouse>()
            .HasMany(resource => resource.ChickensReadyForLaying);

        base.OnModelCreating(builder);
    }
}
