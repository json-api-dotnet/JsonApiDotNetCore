using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.Archiving
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class TelevisionDbContext : DbContext
    {
        public DbSet<TelevisionNetwork> Networks { get; set; }
        public DbSet<TelevisionStation> Stations { get; set; }
        public DbSet<TelevisionBroadcast> Broadcasts { get; set; }
        public DbSet<BroadcastComment> Comments { get; set; }

        public TelevisionDbContext(DbContextOptions<TelevisionDbContext> options)
            : base(options)
        {
        }
    }
}
