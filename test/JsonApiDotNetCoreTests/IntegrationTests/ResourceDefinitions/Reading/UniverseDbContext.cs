using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class UniverseDbContext(DbContextOptions<UniverseDbContext> options)
    : TestableDbContext(options)
{
    public DbSet<Constellation> Constellations => Set<Constellation>();
    public DbSet<Star> Stars => Set<Star>();
    public DbSet<Planet> Planets => Set<Planet>();
    public DbSet<Moon> Moons => Set<Moon>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Star>()
            .ComplexProperty(star => star.Classification, complexBuilder => complexBuilder.ToJson());

        builder.Entity<Constellation>()
            .ComplexProperty(constellation => constellation.Origin, complexBuilder => complexBuilder.ToJson());

        builder.Entity<Planet>()
            .ComplexProperty(planet => planet.Atmosphere, complexBuilder => complexBuilder.ToJson());

        base.OnModelCreating(builder);
    }
}
