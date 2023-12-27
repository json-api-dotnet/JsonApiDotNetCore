using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace OpenApiTests.ResourceFieldValidation.NullableReferenceTypesOn;

// @formatter:wrap_chained_method_calls chop_always

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class NrtOnDbContext(DbContextOptions<NrtOnDbContext> options) : TestableDbContext(options)
{
    public DbSet<NrtOnResource> Resources => Set<NrtOnResource>();
    public DbSet<NrtOnEmpty> Empties => Set<NrtOnEmpty>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<NrtOnResource>()
            .HasOne(resource => resource.NonNullableToOne)
            .WithOne()
            .HasForeignKey<NrtOnResource>("NonNullableToOneId");

        builder.Entity<NrtOnResource>()
            .HasOne(resource => resource.RequiredNonNullableToOne)
            .WithOne()
            .HasForeignKey<NrtOnResource>("RequiredNonNullableToOneId");

        builder.Entity<NrtOnResource>()
            .HasOne(resource => resource.NullableToOne)
            .WithOne()
            .HasForeignKey<NrtOnResource>("NullableToOneId");

        builder.Entity<NrtOnResource>()
            .HasOne(resource => resource.RequiredNullableToOne)
            .WithOne()
            .HasForeignKey<NrtOnResource>("RequiredNullableToOneId");

        builder.Entity<NrtOnResource>()
            .HasMany(resource => resource.ToMany)
            .WithOne()
            .HasForeignKey("ToManyId");

        builder.Entity<NrtOnResource>()
            .HasMany(resource => resource.RequiredToMany)
            .WithOne()
            .HasForeignKey("RequiredToManyId");

        base.OnModelCreating(builder);
    }
}
