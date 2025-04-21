using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace OpenApiTests.ClientIdGenerationModes;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ClientIdGenerationDbContext(DbContextOptions<ClientIdGenerationDbContext> options)
    : TestableDbContext(options)
{
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Game> Games => Set<Game>();
    public DbSet<PlayerGroup> PlayerGroups => Set<PlayerGroup>();
}
