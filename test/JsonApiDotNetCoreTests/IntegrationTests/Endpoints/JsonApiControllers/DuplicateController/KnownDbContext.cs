using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.Endpoints.JsonApiControllers.DuplicateController;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class KnownDbContext(DbContextOptions<KnownDbContext> options)
    : TestableDbContext(options)
{
    public DbSet<KnownResource> KnownResources => Set<KnownResource>();
}
