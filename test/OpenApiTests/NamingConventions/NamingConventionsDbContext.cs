using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace OpenApiTests.NamingConventions;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class NamingConventionsDbContext : TestableDbContext
{
    public DbSet<Supermarket> Supermarkets => Set<Supermarket>();

    public NamingConventionsDbContext(DbContextOptions<NamingConventionsDbContext> options)
        : base(options)
    {
    }
}
