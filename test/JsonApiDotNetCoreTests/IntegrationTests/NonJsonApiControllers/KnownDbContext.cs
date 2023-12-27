using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.NonJsonApiControllers;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class KnownDbContext(DbContextOptions<KnownDbContext> options) : TestableDbContext(options)
{
    public DbSet<KnownResource> KnownResources => Set<KnownResource>();
}
