using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace OpenApiTests.ResourceFieldValidation.NullableReferenceTypesDisabled;

// @formatter:wrap_chained_method_calls chop_always

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class NullableReferenceTypesDisabledDbContext : TestableDbContext
{
    public DbSet<NrtDisabledResource> NrtDisabledResources => Set<NrtDisabledResource>();

    public NullableReferenceTypesDisabledDbContext(DbContextOptions<NullableReferenceTypesDisabledDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<NrtDisabledResource>()
            .HasOne(resource => resource.ToOne);

        builder.Entity<NrtDisabledResource>()
            .HasOne(resource => resource.RequiredToOne);

        builder.Entity<NrtDisabledResource>()
            .HasMany(resource => resource.ToMany);

        builder.Entity<NrtDisabledResource>()
            .HasMany(resource => resource.RequiredToMany);

        base.OnModelCreating(builder);
    }
}
