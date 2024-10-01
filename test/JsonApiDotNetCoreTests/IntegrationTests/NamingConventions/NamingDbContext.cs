using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.NamingConventions;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class NamingDbContext(DbContextOptions<NamingDbContext> options)
    : TestableDbContext(options)
{
    public DbSet<SwimmingPool> SwimmingPools => Set<SwimmingPool>();
    public DbSet<WaterSlide> WaterSlides => Set<WaterSlide>();
    public DbSet<DivingBoard> DivingBoards => Set<DivingBoard>();
}
