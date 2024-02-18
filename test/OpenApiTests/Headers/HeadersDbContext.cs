using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace OpenApiTests.Headers;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class HeadersDbContext(DbContextOptions<HeadersDbContext> options) : TestableDbContext(options)
{
    public DbSet<Country> Countries => Set<Country>();
}
