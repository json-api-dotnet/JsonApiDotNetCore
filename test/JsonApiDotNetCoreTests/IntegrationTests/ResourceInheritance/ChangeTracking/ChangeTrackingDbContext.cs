using JetBrains.Annotations;
using JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Models;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.ChangeTracking;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ChangeTrackingDbContext(DbContextOptions<ChangeTrackingDbContext> options) : ResourceInheritanceDbContext(options)
{
    public DbSet<AlwaysMovingTandem> AlwaysMovingTandems => Set<AlwaysMovingTandem>();
}
