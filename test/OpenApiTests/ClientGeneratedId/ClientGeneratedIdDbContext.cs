using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace OpenApiTests.ClientGeneratedId;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ClientGeneratedIdDbContext(DbContextOptions<ClientGeneratedIdDbContext> options) : TestableDbContext(options)
{
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Game> Games => Set<Game>();
}
