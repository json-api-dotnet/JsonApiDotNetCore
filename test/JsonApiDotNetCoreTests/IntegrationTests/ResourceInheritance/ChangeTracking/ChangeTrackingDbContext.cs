using JetBrains.Annotations;
using JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Models;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.ChangeTracking;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ChangeTrackingDbContext : ResourceInheritanceDbContext
{
    public DbSet<AlwaysMovingTandem> AlwaysMovingTandems => Set<AlwaysMovingTandem>();

    public ChangeTrackingDbContext(DbContextOptions<ChangeTrackingDbContext> options)
        : base(options)
    {
    }
}
