using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace OpenApiTests.ResourceFieldValidation.NullableReferenceTypesOff;

// @formatter:wrap_chained_method_calls chop_always

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class NrtOffDbContext : TestableDbContext
{
    public DbSet<NrtOffResource> NrtOffResources => Set<NrtOffResource>();

    public NrtOffDbContext(DbContextOptions<NrtOffDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<NrtOffResource>()
            .HasOne(resource => resource.ToOne);

        builder.Entity<NrtOffResource>()
            .HasOne(resource => resource.RequiredToOne);

        builder.Entity<NrtOffResource>()
            .HasMany(resource => resource.ToMany);

        builder.Entity<NrtOffResource>()
            .HasMany(resource => resource.RequiredToMany);

        base.OnModelCreating(builder);
    }
}
