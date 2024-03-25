using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace OpenApiTests.Links;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class LinkDbContext(DbContextOptions<LinkDbContext> options) : TestableDbContext(options)
{
    public DbSet<Vacation> Vacations => Set<Vacation>();
    public DbSet<Accommodation> Accommodations => Set<Accommodation>();
    public DbSet<Transport> Transports => Set<Transport>();
    public DbSet<Excursion> Excursions => Set<Excursion>();
}
