using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_always

namespace OpenApiTests.SchemaProperties.NullableReferenceTypesEnabled.RelationshipsObject;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class NullableReferenceTypesEnabledDbContext : TestableDbContext
{
    public DbSet<NrtEnabledModel> NrtEnabledModel => Set<NrtEnabledModel>();
    public DbSet<RelationshipModel> RelationshipModel => Set<RelationshipModel>();

    public NullableReferenceTypesEnabledDbContext(DbContextOptions<NullableReferenceTypesEnabledDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<NrtEnabledModel>()
            .HasOne(resource => resource.HasOne);

        builder.Entity<NrtEnabledModel>()
            .HasOne(resource => resource.RequiredHasOne);

        builder.Entity<NrtEnabledModel>()
            .HasMany(resource => resource.HasMany);

        builder.Entity<NrtEnabledModel>()
            .HasMany(resource => resource.RequiredHasMany);

        builder.Entity<NrtEnabledModel>()
            .HasOne(resource => resource.NullableHasOne);

        builder.Entity<NrtEnabledModel>()
            .HasOne(resource => resource.NullableRequiredHasOne);

        base.OnModelCreating(builder);
    }
}

