using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.Endpoints.JsonApiControllers.CustomActionMethods;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class CoffeeDbContext(DbContextOptions<CoffeeDbContext> options)
    : TestableDbContext(options)
{
    public DbSet<CupOfCoffee> CupsOfCoffee => Set<CupOfCoffee>();
}
