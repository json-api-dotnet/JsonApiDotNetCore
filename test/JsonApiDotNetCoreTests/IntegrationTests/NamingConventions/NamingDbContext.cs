using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.NamingConventions;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class NamingDbContext : TestableDbContext
{
    public DbSet<SwimmingPool> SwimmingPools => Set<SwimmingPool>();
    public DbSet<WaterSlide> WaterSlides => Set<WaterSlide>();
    public DbSet<DivingBoard> DivingBoards => Set<DivingBoard>();

    public NamingDbContext(DbContextOptions<NamingDbContext> options)
        : base(options)
    {
    }
}
