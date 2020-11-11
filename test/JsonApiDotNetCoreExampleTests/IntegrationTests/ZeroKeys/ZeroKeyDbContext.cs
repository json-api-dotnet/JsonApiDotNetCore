using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ZeroKeys
{
    public sealed class ZeroKeyDbContext : DbContext
    {
        public DbSet<Game> Games { get; set; }
        public DbSet<Player> Players { get; set; }

        public ZeroKeyDbContext(DbContextOptions<ZeroKeyDbContext> options) : base(options)
        {
        }
    }
}
