using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace OpenApiTests.MixedControllers;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class CoffeeDbContext(DbContextOptions<CoffeeDbContext> options)
    : TestableDbContext(options)
{
    public DbSet<CupOfCoffee> CupsOfCoffee => Set<CupOfCoffee>();
}
