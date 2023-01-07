using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace OpenApiTests.ResourceFieldValidation.NullableReferenceTypesOn;

// @formatter:wrap_chained_method_calls chop_always

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class NrtOnDbContext : TestableDbContext
{
    public DbSet<NrtOnResource> NrtOnResources => Set<NrtOnResource>();

    public NrtOnDbContext(DbContextOptions<NrtOnDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<NrtOnResource>()
            .HasOne(resource => resource.NonNullableToOne);

        builder.Entity<NrtOnResource>()
            .HasOne(resource => resource.RequiredNonNullableToOne);

        builder.Entity<NrtOnResource>()
            .HasOne(resource => resource.NullableToOne);

        builder.Entity<NrtOnResource>()
            .HasOne(resource => resource.RequiredNullableToOne);

        builder.Entity<NrtOnResource>()
            .HasMany(resource => resource.ToMany);

        builder.Entity<NrtOnResource>()
            .HasMany(resource => resource.RequiredToMany);

        base.OnModelCreating(builder);
    }
}
