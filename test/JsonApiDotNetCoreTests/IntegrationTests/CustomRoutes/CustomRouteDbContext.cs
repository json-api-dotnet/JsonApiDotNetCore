using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.CustomRoutes;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class CustomRouteDbContext(DbContextOptions<CustomRouteDbContext> options) : TestableDbContext(options)
{
    public DbSet<Town> Towns => Set<Town>();
    public DbSet<Civilian> Civilians => Set<Civilian>();
}
