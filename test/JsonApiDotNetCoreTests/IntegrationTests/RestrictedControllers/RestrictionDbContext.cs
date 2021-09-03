using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.RestrictedControllers
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class RestrictionDbContext : DbContext
    {
        public DbSet<Table> Tables { get; set; }
        public DbSet<Chair> Chairs { get; set; }
        public DbSet<Sofa> Sofas { get; set; }
        public DbSet<Bed> Beds { get; set; }

        public RestrictionDbContext(DbContextOptions<RestrictionDbContext> options)
            : base(options)
        {
        }
    }
}
