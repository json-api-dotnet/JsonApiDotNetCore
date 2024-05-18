using Bogus;
using JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Models;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance;

internal sealed class ResourceInheritanceFakers
{
    private readonly Lazy<Faker<Bike>> _lazyBikeFaker = new(() => new Faker<Bike>()
        .MakeDeterministic()
        .RuleFor(bike => bike.Weight, faker => faker.Random.Decimal(10, 30))
        .RuleFor(bike => bike.RequiresDriverLicense, _ => false)
        .RuleFor(bike => bike.GearCount, faker => faker.Random.Int(1, 10)));

    private readonly Lazy<Faker<Tandem>> _lazyTandemFaker = new(() => new Faker<Tandem>()
        .MakeDeterministic()
        .RuleFor(tandem => tandem.Weight, faker => faker.Random.Decimal(20, 50))
        .RuleFor(tandem => tandem.RequiresDriverLicense, _ => false)
        .RuleFor(tandem => tandem.GearCount, faker => faker.Random.Int(1, 10))
        .RuleFor(tandem => tandem.PassengerCount, faker => faker.Random.Int(2, 150)));

    private readonly Lazy<Faker<AlwaysMovingTandem>> _lazyAlwaysMovingTandemFaker = new(() => new Faker<AlwaysMovingTandem>()
        .MakeDeterministic()
        .RuleFor(movingTandem => movingTandem.Weight, faker => faker.Random.Decimal(10, 30))
        .RuleFor(movingTandem => movingTandem.RequiresDriverLicense, _ => false)
        .RuleFor(movingTandem => movingTandem.GearCount, faker => faker.Random.Int(1, 10)));

    private readonly Lazy<Faker<Car>> _lazyCarFaker = new(() => new Faker<Car>()
        .MakeDeterministic()
        .RuleFor(car => car.Weight, faker => faker.Random.Decimal(1200, 2000))
        .RuleFor(car => car.RequiresDriverLicense, _ => true)
        .RuleFor(car => car.LicensePlate, faker => faker.Random.Replace("??-??-##"))
        .RuleFor(car => car.SeatCount, faker => faker.Random.Int(2, 8)));

    private readonly Lazy<Faker<Truck>> _lazyTruckFaker = new(() => new Faker<Truck>()
        .MakeDeterministic()
        .RuleFor(truck => truck.Weight, faker => faker.Random.Decimal(1200, 12_000))
        .RuleFor(truck => truck.RequiresDriverLicense, _ => true)
        .RuleFor(truck => truck.LicensePlate, faker => faker.Random.Replace("??-??-##"))
        .RuleFor(truck => truck.LoadingCapacity, faker => faker.Random.Decimal(500, 25_000)));

    private readonly Lazy<Faker<CarbonWheel>> _lazyCarbonWheelFaker = new(() => new Faker<CarbonWheel>()
        .MakeDeterministic()
        .RuleFor(carbonWheel => carbonWheel.Radius, faker => faker.Random.Decimal(50, 100))
        .RuleFor(carbonWheel => carbonWheel.HasTube, faker => faker.Random.Bool()));

    private readonly Lazy<Faker<ChromeWheel>> _lazyChromeWheelFaker = new(() => new Faker<ChromeWheel>()
        .MakeDeterministic()
        .RuleFor(chromeWheel => chromeWheel.Radius, faker => faker.Random.Decimal(50, 100))
        .RuleFor(chromeWheel => chromeWheel.PaintColor, faker => faker.Internet.Color()));

    private readonly Lazy<Faker<GasolineEngine>> _lazyGasolineEngineFaker = new(() => new Faker<GasolineEngine>()
        .MakeDeterministic()
        .RuleFor(gasolineEngine => gasolineEngine.IsHydrocarbonBased, faker => faker.Random.Bool())
        .RuleFor(gasolineEngine => gasolineEngine.Capacity, faker => faker.Random.Decimal(10, 500))
        .RuleFor(gasolineEngine => gasolineEngine.SerialCode, faker => faker.Commerce.Ean8())
        .RuleFor(gasolineEngine => gasolineEngine.Volatility, faker => faker.Random.Decimal(1, 100)));

    private readonly Lazy<Faker<Cylinder>> _lazyCylinderFaker = new(() => new Faker<Cylinder>()
        .MakeDeterministic()
        .RuleFor(cylinder => cylinder.SparkPlugCount, faker => faker.Random.Int(1, 5)));

    private readonly Lazy<Faker<DieselEngine>> _lazyDieselEngineFaker = new(() => new Faker<DieselEngine>()
        .MakeDeterministic()
        .RuleFor(dieselEngine => dieselEngine.IsHydrocarbonBased, faker => faker.Random.Bool())
        .RuleFor(dieselEngine => dieselEngine.Capacity, faker => faker.Random.Decimal(10, 500))
        .RuleFor(dieselEngine => dieselEngine.SerialCode, faker => faker.Commerce.Ean8())
        .RuleFor(dieselEngine => dieselEngine.Viscosity, faker => faker.Random.Decimal(1, 100)));

    private readonly Lazy<Faker<VehicleManufacturer>> _lazyVehicleManufacturerFaker = new(() => new Faker<VehicleManufacturer>()
        .MakeDeterministic()
        .RuleFor(vehicleManufacturer => vehicleManufacturer.Name, faker => faker.Company.CompanyName()));

    private readonly Lazy<Faker<BicycleLight>> _lazyBicycleLightFaker = new(() => new Faker<BicycleLight>()
        .MakeDeterministic()
        .RuleFor(bicycleLight => bicycleLight.Color, faker => faker.Internet.Color()));

    private readonly Lazy<Faker<Box>> _lazyBoxFaker = new(() => new Faker<Box>()
        .MakeDeterministic()
        .RuleFor(box => box.Width, faker => faker.Random.Decimal(1, 100))
        .RuleFor(box => box.Height, faker => faker.Random.Decimal(1, 100))
        .RuleFor(box => box.Depth, faker => faker.Random.Decimal(1, 100)));

    private readonly Lazy<Faker<NavigationSystem>> _lazyNavigationSystemFaker = new(() => new Faker<NavigationSystem>()
        .MakeDeterministic()
        .RuleFor(navigationSystem => navigationSystem.ModelType, faker => faker.Commerce.ProductName()));

    private readonly Lazy<Faker<GenericFeature>> _lazyGenericFeatureFaker = new(() => new Faker<GenericFeature>()
        .MakeDeterministic()
        .RuleFor(genericFeature => genericFeature.Description, faker => faker.Lorem.Sentence()));

    private readonly Lazy<Faker<StringProperty>> _lazyStringPropertyFaker = new(() => new Faker<StringProperty>()
        .MakeDeterministic()
        .RuleFor(stringProperty => stringProperty.Name, faker => faker.Lorem.Word()));

    private readonly Lazy<Faker<StringValue>> _lazyStringValueFaker = new(() => new Faker<StringValue>()
        .MakeDeterministic()
        .RuleFor(stringValue => stringValue.Content, faker => faker.Lorem.Lines(1)));

    private readonly Lazy<Faker<NumberProperty>> _lazyNumberPropertyFaker = new(() => new Faker<NumberProperty>()
        .MakeDeterministic()
        .RuleFor(numberProperty => numberProperty.Name, faker => faker.Lorem.Word()));

    private readonly Lazy<Faker<NumberValue>> _lazyNumberValueFaker = new(() => new Faker<NumberValue>()
        .MakeDeterministic()
        .RuleFor(numberValue => numberValue.Content, faker => faker.Random.Decimal()));

    public Faker<Bike> Bike => _lazyBikeFaker.Value;
    public Faker<Tandem> Tandem => _lazyTandemFaker.Value;
    public Faker<AlwaysMovingTandem> AlwaysMovingTandem => _lazyAlwaysMovingTandemFaker.Value;
    public Faker<Car> Car => _lazyCarFaker.Value;
    public Faker<Truck> Truck => _lazyTruckFaker.Value;
    public Faker<CarbonWheel> CarbonWheel => _lazyCarbonWheelFaker.Value;
    public Faker<ChromeWheel> ChromeWheel => _lazyChromeWheelFaker.Value;
    public Faker<GasolineEngine> GasolineEngine => _lazyGasolineEngineFaker.Value;
    public Faker<Cylinder> Cylinder => _lazyCylinderFaker.Value;
    public Faker<DieselEngine> DieselEngine => _lazyDieselEngineFaker.Value;
    public Faker<VehicleManufacturer> VehicleManufacturer => _lazyVehicleManufacturerFaker.Value;
    public Faker<BicycleLight> BicycleLight => _lazyBicycleLightFaker.Value;
    public Faker<Box> Box => _lazyBoxFaker.Value;
    public Faker<NavigationSystem> NavigationSystem => _lazyNavigationSystemFaker.Value;
    public Faker<GenericFeature> GenericFeature => _lazyGenericFeatureFaker.Value;
    public Faker<StringProperty> StringProperty => _lazyStringPropertyFaker.Value;
    public Faker<StringValue> StringValue => _lazyStringValueFaker.Value;
    public Faker<NumberProperty> NumberProperty => _lazyNumberPropertyFaker.Value;
    public Faker<NumberValue> NumberValue => _lazyNumberValueFaker.Value;
}
