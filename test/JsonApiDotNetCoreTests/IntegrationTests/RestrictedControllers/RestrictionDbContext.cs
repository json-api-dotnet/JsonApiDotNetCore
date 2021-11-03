using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.RestrictedControllers
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class RestrictionDbContext : DbContext
    {
        public DbSet<Table> Tables => Set<Table>();
        public DbSet<Chair> Chairs => Set<Chair>();
        public DbSet<Sofa> Sofas => Set<Sofa>();
        public DbSet<Bed> Beds => Set<Bed>();

        public RestrictionDbContext(DbContextOptions<RestrictionDbContext> options)
            : base(options)
        {
        }
    }
}
