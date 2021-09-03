using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.NamingConventions
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class SwimmingDbContext : DbContext
    {
        public DbSet<SwimmingPool> SwimmingPools { get; set; }
        public DbSet<WaterSlide> WaterSlides { get; set; }
        public DbSet<DivingBoard> DivingBoards { get; set; }

        public SwimmingDbContext(DbContextOptions<SwimmingDbContext> options)
            : base(options)
        {
        }
    }
}
