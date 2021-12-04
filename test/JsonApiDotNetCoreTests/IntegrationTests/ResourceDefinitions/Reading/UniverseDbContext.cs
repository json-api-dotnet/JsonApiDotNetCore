using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class UniverseDbContext : DbContext
{
    public DbSet<Star> Stars => Set<Star>();
    public DbSet<Planet> Planets => Set<Planet>();
    public DbSet<Moon> Moons => Set<Moon>();

    public UniverseDbContext(DbContextOptions<UniverseDbContext> options)
        : base(options)
    {
    }
}
