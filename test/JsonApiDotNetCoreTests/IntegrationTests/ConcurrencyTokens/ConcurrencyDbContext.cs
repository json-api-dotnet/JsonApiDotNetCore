using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreTests.IntegrationTests.ConcurrencyTokens
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class ConcurrencyDbContext : DbContext
    {
        public DbSet<Disk> Disks => Set<Disk>();
        public DbSet<Partition> Partitions => Set<Partition>();

        public ConcurrencyDbContext(DbContextOptions<ConcurrencyDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // https://www.npgsql.org/efcore/modeling/concurrency.html

            builder.Entity<Disk>()
                .UseXminAsConcurrencyToken();

            builder.Entity<Partition>()
                .UseXminAsConcurrencyToken();
        }
    }
}
