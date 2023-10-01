using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace OpenApiTests.ResourceFieldValidation.NullableReferenceTypesOff;

// @formatter:wrap_chained_method_calls chop_always

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class NrtOffDbContext : TestableDbContext
{
    public DbSet<NrtOffResource> Resources => Set<NrtOffResource>();
    public DbSet<NrtOffEmpty> Empties => Set<NrtOffEmpty>();

    public NrtOffDbContext(DbContextOptions<NrtOffDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<NrtOffResource>()
            .HasOne(resource => resource.ToOne)
            .WithOne()
            .HasForeignKey<NrtOffResource>("ToOneId");

        builder.Entity<NrtOffResource>()
            .HasOne(resource => resource.RequiredToOne)
            .WithOne()
            .HasForeignKey<NrtOffResource>("RequiredToOneId");

        builder.Entity<NrtOffResource>()
            .HasMany(resource => resource.ToMany)
            .WithOne()
            .HasForeignKey("ToManyId");

        builder.Entity<NrtOffResource>()
            .HasMany(resource => resource.RequiredToMany)
            .WithOne()
            .HasForeignKey("RequiredToManyId");

        base.OnModelCreating(builder);
    }
}
