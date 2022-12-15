using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class UniverseDbContext : TestableDbContext
{
    public DbSet<Star> Stars => Set<Star>();
    public DbSet<Planet> Planets => Set<Planet>();
    public DbSet<Moon> Moons => Set<Moon>();

    public UniverseDbContext(DbContextOptions<UniverseDbContext> options)
        : base(options)
    {
    }
}
