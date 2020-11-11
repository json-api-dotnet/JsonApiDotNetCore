using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ZeroKeys
{
    public sealed class ZeroKeyDbContext : DbContext
    {
        public DbSet<Game> Games { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Map> Maps { get; set; }

        public ZeroKeyDbContext(DbContextOptions<ZeroKeyDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Game>()
                .HasMany(game => game.Maps)
                .WithOne(map => map.Game);

            builder.Entity<Player>()
                .HasOne(player => player.ActiveGame)
                .WithMany(game => game.ActivePlayers);
        }
    }
}
