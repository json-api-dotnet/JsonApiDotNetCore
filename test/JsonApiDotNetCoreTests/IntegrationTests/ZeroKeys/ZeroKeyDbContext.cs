using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreTests.IntegrationTests.ZeroKeys;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ZeroKeyDbContext : DbContext
{
    public DbSet<Game> Games => Set<Game>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Map> Maps => Set<Map>();

    public ZeroKeyDbContext(DbContextOptions<ZeroKeyDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Game>()
            .HasMany(game => game.Maps)
            .WithOne(map => map.Game!);

        builder.Entity<Player>()
            .HasOne(player => player.ActiveGame)
            .WithMany(game => game.ActivePlayers);
    }
}
