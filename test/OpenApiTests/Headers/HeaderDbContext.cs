using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace OpenApiTests.Headers;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class HeaderDbContext(DbContextOptions<HeaderDbContext> options)
    : TestableDbContext(options)
{
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Language> Languages => Set<Language>();
}
