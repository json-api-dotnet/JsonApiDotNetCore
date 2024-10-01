using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace OpenApiTests.NamingConventions;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class NamingConventionDbContext(DbContextOptions<NamingConventionDbContext> options)
    : TestableDbContext(options)
{
    public DbSet<Supermarket> Supermarkets => Set<Supermarket>();
}
