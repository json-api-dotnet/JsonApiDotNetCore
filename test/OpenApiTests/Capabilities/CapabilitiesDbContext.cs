using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_always

namespace OpenApiTests.Capabilities;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class CapabilitiesDbContext(DbContextOptions<CapabilitiesDbContext> options)
    : TestableDbContext(options)
{
    public DbSet<AllowViewCapability> AllowViewCapabilities => Set<AllowViewCapability>();
    public DbSet<AllowCreateChangeCapability> AllowCreateChangeCapabilities => Set<AllowCreateChangeCapability>();
    public DbSet<AllowSetCapability> AllowSetCapabilities => Set<AllowSetCapability>();
    public DbSet<AllowAddRemoveCapability> AllowAddRemoveCapabilities => Set<AllowAddRemoveCapability>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<AllowViewCapability>()
            .HasMany(capability => capability.ChildrenViewOn)
            .WithOne(capability => capability.ParentViewOn);

        builder.Entity<AllowViewCapability>()
            .HasMany(capability => capability.ChildrenViewOff)
            .WithOne(capability => capability.ParentViewOff);

        builder.Entity<AllowSetCapability>()
            .HasMany(capability => capability.ChildrenSetOn)
            .WithOne(capability => capability.ParentSetOn);

        builder.Entity<AllowSetCapability>()
            .HasMany(capability => capability.ChildrenSetOff)
            .WithOne(capability => capability.ParentSetOff);

        base.OnModelCreating(builder);
    }
}
