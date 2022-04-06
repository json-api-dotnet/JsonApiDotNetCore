using JetBrains.Annotations;
using JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Models;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.TablePerType;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class TablePerTypeDbContext : ResourceInheritanceDbContext
{
    public TablePerTypeDbContext(DbContextOptions<TablePerTypeDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Vehicle>().ToTable("Vehicles");
        builder.Entity<Bike>().ToTable("Bikes");
        builder.Entity<Tandem>().ToTable("Tandems");
        builder.Entity<MotorVehicle>().ToTable("MotorVehicles");
        builder.Entity<Car>().ToTable("Cars");
        builder.Entity<Truck>().ToTable("Trucks");

        builder.Entity<Wheel>().ToTable("Wheels");
        builder.Entity<CarbonWheel>().ToTable("CarbonWheels");
        builder.Entity<ChromeWheel>().ToTable("ChromeWheels");

        builder.Entity<Engine>().ToTable("Engines");
        builder.Entity<GasolineEngine>().ToTable("GasolineEngines");
        builder.Entity<DieselEngine>().ToTable("DieselEngines");

        builder.Entity<GenericProperty>().ToTable("GenericProperties");
        builder.Entity<StringProperty>().ToTable("StringProperties");
        builder.Entity<NumberProperty>().ToTable("NumberProperties");
    }
}
