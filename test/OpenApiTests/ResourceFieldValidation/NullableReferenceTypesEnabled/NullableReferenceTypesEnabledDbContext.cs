using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace OpenApiTests.ResourceFieldValidation.NullableReferenceTypesEnabled;

// @formatter:wrap_chained_method_calls chop_always

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class NullableReferenceTypesEnabledDbContext : TestableDbContext
{
    public DbSet<NrtEnabledResource> NullableReferenceTypesEnabledResources => Set<NrtEnabledResource>();

    public NullableReferenceTypesEnabledDbContext(DbContextOptions<NullableReferenceTypesEnabledDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<NrtEnabledResource>()
            .HasOne(resource => resource.NonNullableToOne);

        builder.Entity<NrtEnabledResource>()
            .HasOne(resource => resource.RequiredNonNullableToOne);

        builder.Entity<NrtEnabledResource>()
            .HasOne(resource => resource.NullableToOne);

        builder.Entity<NrtEnabledResource>()
            .HasOne(resource => resource.RequiredNullableToOne);

        builder.Entity<NrtEnabledResource>()
            .HasMany(resource => resource.ToMany);

        builder.Entity<NrtEnabledResource>()
            .HasMany(resource => resource.RequiredToMany);

        base.OnModelCreating(builder);
    }
}
