using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using OpenApiTests.SchemaProperties.NullableReferenceTypesEnabled.RelationshipsObject;
using TestBuildingBlocks;

namespace OpenApiTests.SchemaProperties.NullableReferenceTypesEnabled;

// @formatter:wrap_chained_method_calls chop_always

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class NullableReferenceTypesEnabledDbContext : TestableDbContext
{
    public DbSet<Cow> Cow => Set<Cow>();
    public DbSet<CowStable> CowStable => Set<CowStable>();

    public NullableReferenceTypesEnabledDbContext(DbContextOptions<NullableReferenceTypesEnabledDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<CowStable>()
            .HasOne(resource => resource.OldestCow);

        builder.Entity<CowStable>()
            .HasOne(resource => resource.FirstCow);

        builder.Entity<CowStable>()
            .HasOne(resource => resource.AlbinoCow);

        builder.Entity<CowStable>()
            .HasOne(resource => resource.FavoriteCow);

        builder.Entity<CowStable>()
            .HasMany(resource => resource.AllCows);

        builder.Entity<CowStable>()
            .HasMany(resource => resource.CowsReadyForMilking);

        base.OnModelCreating(builder);
    }
}

