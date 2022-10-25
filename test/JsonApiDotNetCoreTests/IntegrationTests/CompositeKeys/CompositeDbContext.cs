using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreTests.IntegrationTests.CompositeKeys;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class CompositeDbContext : TestableDbContext
{
    public DbSet<Car> Cars => Set<Car>();
    public DbSet<Engine> Engines => Set<Engine>();
    public DbSet<Dealership> Dealerships => Set<Dealership>();

    public CompositeDbContext(DbContextOptions<CompositeDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Car>()
            .HasKey(car => new
            {
                car.RegionId,
                car.LicensePlate
            });

        builder.Entity<Engine>()
            .HasOne(engine => engine.Car)
            .WithOne(car => car.Engine)
            .HasForeignKey<Engine>();

        builder.Entity<Dealership>()
            .HasMany(dealership => dealership.Inventory)
            .WithOne(car => car.Dealership!);

        builder.Entity<Car>()
            .HasMany(car => car.PreviousDealerships)
            .WithMany(dealership => dealership.SoldCars);
    }
}
