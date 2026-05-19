using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace OpenApiTests.CustomRoutes;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class CustomRouteDbContext(DbContextOptions<CustomRouteDbContext> options)
    : TestableDbContext(options)
{
    public DbSet<Election> Elections => Set<Election>();
    public DbSet<Candidate> Candidates => Set<Candidate>();
    public DbSet<Ballot> Ballots => Set<Ballot>();
}
