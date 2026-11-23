using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreTests.IntegrationTests.NamingConventions;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class NamingDbContext(DbContextOptions<NamingDbContext> options)
    : TestableDbContext(options)
{
    public DbSet<SwimmingPool> SwimmingPools => Set<SwimmingPool>();
    public DbSet<WaterSlide> WaterSlides => Set<WaterSlide>();
    public DbSet<DivingBoard> DivingBoards => Set<DivingBoard>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<DivingBoard>()
            .ComplexProperty(divingBoard => divingBoard.Schedule, complexBuilder => complexBuilder.ToJson());

        builder.Entity<DivingBoard>()
            .ComplexCollection(divingBoard => divingBoard.Reservations, complexBuilder => complexBuilder.ToJson());

        base.OnModelCreating(builder);
    }
}
