using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_always

namespace OpenApiTests.SchemaProperties.NullableReferenceTypesDisabled.RelationshipsObject;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class NullableReferenceTypesDisabledDbContext : TestableDbContext
{
    public DbSet<NrtDisabledModel> NrtDisabledModel => Set<NrtDisabledModel>();
    public DbSet<RelationshipModel> RelationshipModel => Set<RelationshipModel>();

    public NullableReferenceTypesDisabledDbContext(DbContextOptions<NullableReferenceTypesDisabledDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<NrtDisabledModel>()
            .HasOne(resource => resource.HasOne);
        builder.Entity<NrtDisabledModel>()
            .HasOne(resource => resource.RequiredHasOne);
        builder.Entity<NrtDisabledModel>()
            .HasMany(resource => resource.HasMany);
        builder.Entity<NrtDisabledModel>()
            .HasMany(resource => resource.RequiredHasMany);

        base.OnModelCreating(builder);
    }
}
