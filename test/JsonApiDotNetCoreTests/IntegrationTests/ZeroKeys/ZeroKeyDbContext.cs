using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreTests.IntegrationTests.ZeroKeys;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ZeroKeyDbContext(DbContextOptions<ZeroKeyDbContext> options)
    : TestableDbContext(options)
{
    public DbSet<Game> Games => Set<Game>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Map> Maps => Set<Map>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Game>()
            .HasMany(game => game.Maps)
            .WithOne(map => map.Game);

        builder.Entity<Player>()
            .HasOne(player => player.ActiveGame)
            .WithMany(game => game.ActivePlayers);

        base.OnModelCreating(builder);
    }
}
