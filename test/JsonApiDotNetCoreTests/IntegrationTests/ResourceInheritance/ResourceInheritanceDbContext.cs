using JetBrains.Annotations;
using JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Models;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public abstract class ResourceInheritanceDbContext : TestableDbContext
{
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Bike> Bikes => Set<Bike>();
    public DbSet<Tandem> Tandems => Set<Tandem>();
    public DbSet<MotorVehicle> MotorVehicles => Set<MotorVehicle>();
    public DbSet<Car> Cars => Set<Car>();
    public DbSet<Truck> Trucks => Set<Truck>();

    public DbSet<Wheel> Wheels => Set<Wheel>();
    public DbSet<CarbonWheel> CarbonWheels => Set<CarbonWheel>();
    public DbSet<ChromeWheel> ChromeWheels => Set<ChromeWheel>();

    public DbSet<Engine> Engines => Set<Engine>();
    public DbSet<GasolineEngine> GasolineEngines => Set<GasolineEngine>();
    public DbSet<DieselEngine> DieselEngines => Set<DieselEngine>();

    public DbSet<GenericProperty> GenericProperties => Set<GenericProperty>();
    public DbSet<StringProperty> StringProperties => Set<StringProperty>();
    public DbSet<NumberProperty> NumberProperties => Set<NumberProperty>();

    public DbSet<VehicleManufacturer> VehicleManufacturers => Set<VehicleManufacturer>();
    public DbSet<Box> Boxes => Set<Box>();
    public DbSet<BicycleLight> BicycleLights => Set<BicycleLight>();
    public DbSet<NavigationSystem> NavigationSystems => Set<NavigationSystem>();
    public DbSet<GenericFeature> GenericFeatures => Set<GenericFeature>();

    protected ResourceInheritanceDbContext(DbContextOptions options)
        : base(options)
    {
    }
}
