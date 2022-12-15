using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.NonJsonApiControllers;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class KnownDbContext : TestableDbContext
{
    public DbSet<KnownResource> KnownResources => Set<KnownResource>();

    public KnownDbContext(DbContextOptions<KnownDbContext> options)
        : base(options)
    {
    }
}
