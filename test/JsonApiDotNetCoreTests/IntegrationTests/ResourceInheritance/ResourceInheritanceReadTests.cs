using System.Globalization;
using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Models;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance;

public abstract class ResourceInheritanceReadTests<TDbContext> : IClassFixture<IntegrationTestContext<TestableStartup<TDbContext>, TDbContext>>
    where TDbContext : ResourceInheritanceDbContext
{
    private readonly IntegrationTestContext<TestableStartup<TDbContext>, TDbContext> _testContext;
    private readonly ResourceInheritanceFakers _fakers = new();

    protected ResourceInheritanceReadTests(IntegrationTestContext<TestableStartup<TDbContext>, TDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<VehicleManufacturersController>();

        testContext.UseController<VehiclesController>();
        testContext.UseController<BikesController>();
        testContext.UseController<TandemsController>();
        testContext.UseController<MotorVehiclesController>();
        testContext.UseController<CarsController>();
        testContext.UseController<TrucksController>();

        testContext.UseController<EnginesController>();
        testContext.UseController<GasolineEnginesController>();
        testContext.UseController<DieselEnginesController>();

        testContext.UseController<WheelsController>();
        testContext.UseController<ChromeWheelsController>();
        testContext.UseController<CarbonWheelsController>();

        testContext.ConfigureServices(services => services.AddResourceDefinition<WheelSortDefinition>());

        var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.UseRelativeLinks = true;
        options.AllowUnknownQueryStringParameters = true;
    }

    [Fact]
    public async Task Can_get_primary_resources_at_abstract_endpoint()
    {
        // Arrange
        Bike bike = _fakers.Bike.GenerateOne();

        Tandem tandem = _fakers.Tandem.GenerateOne();

        Car car = _fakers.Car.GenerateOne();
        car.Engine = _fakers.GasolineEngine.GenerateOne();

        Truck truck = _fakers.Truck.GenerateOne();
        truck.Engine = _fakers.DieselEngine.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Vehicle>();
            dbContext.Vehicles.AddRange(bike, tandem, car, truck);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/vehicles";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Self.Should().Be(route);

        responseDocument.Data.ManyValue.Should().HaveCount(4);

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "bikes").Subject.With(resource =>
        {
            resource.Id.Should().Be(bike.StringId);

            resource.Links.Should().NotBeNull();
            resource.Links.Self.Should().Be($"/bikes/{bike.StringId}");

            resource.Attributes.Should().HaveCount(3);
            resource.Attributes.Should().ContainKey("weight").WhoseValue.Should().Be(bike.Weight);
            resource.Attributes.Should().ContainKey("requiresDriverLicense").WhoseValue.Should().Be(bike.RequiresDriverLicense);
            resource.Attributes.Should().ContainKey("gearCount").WhoseValue.Should().Be(bike.GearCount);

            resource.Relationships.Should().OnlyContainKeys("manufacturer", "wheels", "cargoBox", "lights");

            foreach ((string name, RelationshipObject? value) in resource.Relationships)
            {
                value.Should().NotBeNull();
                value.Links.Should().NotBeNull();
                value.Links.Self.Should().Be($"/bikes/{bike.StringId}/relationships/{name}");
                value.Links.Related.Should().Be($"/bikes/{bike.StringId}/{name}");
            }
        });

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "tandems").Subject.With(resource =>
        {
            resource.Id.Should().Be(tandem.StringId);

            resource.Links.Should().NotBeNull();
            resource.Links.Self.Should().Be($"/tandems/{tandem.StringId}");

            resource.Attributes.Should().HaveCount(4);
            resource.Attributes.Should().ContainKey("weight").WhoseValue.Should().Be(tandem.Weight);
            resource.Attributes.Should().ContainKey("requiresDriverLicense").WhoseValue.Should().Be(tandem.RequiresDriverLicense);
            resource.Attributes.Should().ContainKey("gearCount").WhoseValue.Should().Be(tandem.GearCount);
            resource.Attributes.Should().ContainKey("passengerCount").WhoseValue.Should().Be(tandem.PassengerCount);

            resource.Relationships.Should().OnlyContainKeys("manufacturer", "wheels", "cargoBox", "lights", "foldingDimensions", "features");

            foreach ((string name, RelationshipObject? value) in resource.Relationships)
            {
                value.Should().NotBeNull();
                value.Links.Should().NotBeNull();
                value.Links.Self.Should().Be($"/tandems/{tandem.StringId}/relationships/{name}");
                value.Links.Related.Should().Be($"/tandems/{tandem.StringId}/{name}");
            }
        });

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "cars").Subject.With(resource =>
        {
            resource.Id.Should().Be(car.StringId);

            resource.Links.Should().NotBeNull();
            resource.Links.Self.Should().Be($"/cars/{car.StringId}");

            resource.Attributes.Should().HaveCount(4);
            resource.Attributes.Should().ContainKey("weight").WhoseValue.Should().Be(car.Weight);
            resource.Attributes.Should().ContainKey("requiresDriverLicense").WhoseValue.Should().Be(car.RequiresDriverLicense);
            resource.Attributes.Should().ContainKey("licensePlate").WhoseValue.Should().Be(car.LicensePlate);
            resource.Attributes.Should().ContainKey("seatCount").WhoseValue.Should().Be(car.SeatCount);

            resource.Relationships.Should().OnlyContainKeys("manufacturer", "wheels", "engine", "navigationSystem", "features");

            foreach ((string name, RelationshipObject? value) in resource.Relationships)
            {
                value.Should().NotBeNull();
                value.Links.Should().NotBeNull();
                value.Links.Self.Should().Be($"/cars/{car.StringId}/relationships/{name}");
                value.Links.Related.Should().Be($"/cars/{car.StringId}/{name}");
            }
        });

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "trucks").Subject.With(resource =>
        {
            resource.Id.Should().Be(truck.StringId);

            resource.Links.Should().NotBeNull();
            resource.Links.Self.Should().Be($"/trucks/{truck.StringId}");

            resource.Attributes.Should().HaveCount(4);
            resource.Attributes.Should().ContainKey("weight").WhoseValue.Should().Be(truck.Weight);
            resource.Attributes.Should().ContainKey("requiresDriverLicense").WhoseValue.Should().Be(truck.RequiresDriverLicense);
            resource.Attributes.Should().ContainKey("licensePlate").WhoseValue.Should().Be(truck.LicensePlate);
            resource.Attributes.Should().ContainKey("loadingCapacity").WhoseValue.Should().Be(truck.LoadingCapacity);

            resource.Relationships.Should().OnlyContainKeys("manufacturer", "wheels", "engine", "navigationSystem", "sleepingArea", "features");

            foreach ((string name, RelationshipObject? value) in resource.Relationships)
            {
                value.Should().NotBeNull();
                value.Links.Should().NotBeNull();
                value.Links.Self.Should().Be($"/trucks/{truck.StringId}/relationships/{name}");
                value.Links.Related.Should().Be($"/trucks/{truck.StringId}/{name}");
            }
        });
    }

    [Fact]
    public async Task Can_get_primary_resources_at_concrete_base_endpoint()
    {
        // Arrange
        Bike bike = _fakers.Bike.GenerateOne();
        Tandem tandem = _fakers.Tandem.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Vehicle>();
            dbContext.Vehicles.AddRange(bike, tandem);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/bikes";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Self.Should().Be(route);

        responseDocument.Data.ManyValue.Should().HaveCount(2);

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "bikes").Subject.With(resource =>
        {
            resource.Id.Should().Be(bike.StringId);

            resource.Links.Should().NotBeNull();
            resource.Links.Self.Should().Be($"/bikes/{bike.StringId}");

            resource.Attributes.Should().HaveCount(3);
            resource.Attributes.Should().ContainKey("weight").WhoseValue.Should().Be(bike.Weight);
            resource.Attributes.Should().ContainKey("requiresDriverLicense").WhoseValue.Should().Be(bike.RequiresDriverLicense);
            resource.Attributes.Should().ContainKey("gearCount").WhoseValue.Should().Be(bike.GearCount);

            resource.Relationships.Should().OnlyContainKeys("manufacturer", "wheels", "cargoBox", "lights");

            foreach ((string name, RelationshipObject? value) in resource.Relationships)
            {
                value.Should().NotBeNull();
                value.Links.Should().NotBeNull();
                value.Links.Self.Should().Be($"/bikes/{bike.StringId}/relationships/{name}");
                value.Links.Related.Should().Be($"/bikes/{bike.StringId}/{name}");
            }
        });

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "tandems").Subject.With(resource =>
        {
            resource.Id.Should().Be(tandem.StringId);

            resource.Links.Should().NotBeNull();
            resource.Links.Self.Should().Be($"/tandems/{tandem.StringId}");

            resource.Attributes.Should().HaveCount(4);
            resource.Attributes.Should().ContainKey("weight").WhoseValue.Should().Be(tandem.Weight);
            resource.Attributes.Should().ContainKey("requiresDriverLicense").WhoseValue.Should().Be(tandem.RequiresDriverLicense);
            resource.Attributes.Should().ContainKey("gearCount").WhoseValue.Should().Be(tandem.GearCount);
            resource.Attributes.Should().ContainKey("passengerCount").WhoseValue.Should().Be(tandem.PassengerCount);

            resource.Relationships.Should().OnlyContainKeys("manufacturer", "wheels", "cargoBox", "lights", "foldingDimensions", "features");

            foreach ((string name, RelationshipObject? value) in resource.Relationships)
            {
                value.Should().NotBeNull();
                value.Links.Should().NotBeNull();
                value.Links.Self.Should().Be($"/tandems/{tandem.StringId}/relationships/{name}");
                value.Links.Related.Should().Be($"/tandems/{tandem.StringId}/{name}");
            }
        });
    }

    [Fact]
    public async Task Can_get_primary_resources_at_concrete_derived_endpoint()
    {
        // Arrange
        Bike bike = _fakers.Bike.GenerateOne();
        Tandem tandem = _fakers.Tandem.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Vehicle>();
            dbContext.Vehicles.AddRange(bike, tandem);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/tandems";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Self.Should().Be(route);

        responseDocument.Data.ManyValue.Should().HaveCount(1);

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "tandems").Subject.With(resource =>
        {
            resource.Id.Should().Be(tandem.StringId);

            resource.Links.Should().NotBeNull();
            resource.Links.Self.Should().Be($"/tandems/{tandem.StringId}");

            resource.Attributes.Should().HaveCount(4);
            resource.Attributes.Should().ContainKey("weight").WhoseValue.Should().Be(tandem.Weight);
            resource.Attributes.Should().ContainKey("requiresDriverLicense").WhoseValue.Should().Be(tandem.RequiresDriverLicense);
            resource.Attributes.Should().ContainKey("gearCount").WhoseValue.Should().Be(tandem.GearCount);
            resource.Attributes.Should().ContainKey("passengerCount").WhoseValue.Should().Be(tandem.PassengerCount);

            resource.Relationships.Should().OnlyContainKeys("manufacturer", "wheels", "cargoBox", "lights", "foldingDimensions", "features");

            foreach ((string name, RelationshipObject? value) in resource.Relationships)
            {
                value.Should().NotBeNull();
                value.Links.Should().NotBeNull();
                value.Links.Self.Should().Be($"/tandems/{tandem.StringId}/relationships/{name}");
                value.Links.Related.Should().Be($"/tandems/{tandem.StringId}/{name}");
            }
        });
    }

    [Fact]
    public async Task Can_get_primary_resource_at_abstract_endpoint()
    {
        // Arrange
        Tandem tandem = _fakers.Tandem.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(tandem);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/vehicles/{tandem.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("tandems");
        responseDocument.Data.SingleValue.Id.Should().Be(tandem.StringId);
        responseDocument.Data.SingleValue.Attributes.Should().OnlyContainKeys("weight", "requiresDriverLicense", "gearCount", "passengerCount");

        responseDocument.Data.SingleValue.Relationships.Should().OnlyContainKeys(
            "manufacturer", "wheels", "cargoBox", "lights", "foldingDimensions", "features");
    }

    [Fact]
    public async Task Can_get_primary_resource_at_concrete_base_endpoint()
    {
        // Arrange
        Tandem tandem = _fakers.Tandem.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(tandem);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/bikes/{tandem.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("tandems");
        responseDocument.Data.SingleValue.Id.Should().Be(tandem.StringId);
        responseDocument.Data.SingleValue.Attributes.Should().OnlyContainKeys("weight", "requiresDriverLicense", "gearCount", "passengerCount");

        responseDocument.Data.SingleValue.Relationships.Should().OnlyContainKeys(
            "manufacturer", "wheels", "cargoBox", "lights", "foldingDimensions", "features");
    }

    [Fact]
    public async Task Can_get_primary_resource_at_concrete_derived_endpoint()
    {
        // Arrange
        Tandem tandem = _fakers.Tandem.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(tandem);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/tandems/{tandem.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("tandems");
        responseDocument.Data.SingleValue.Id.Should().Be(tandem.StringId);
        responseDocument.Data.SingleValue.Attributes.Should().OnlyContainKeys("weight", "requiresDriverLicense", "gearCount", "passengerCount");

        responseDocument.Data.SingleValue.Relationships.Should().OnlyContainKeys(
            "manufacturer", "wheels", "cargoBox", "lights", "foldingDimensions", "features");
    }

    [Fact]
    public async Task Can_get_primary_resource_with_derived_includes()
    {
        // Arrange
        VehicleManufacturer manufacturer = _fakers.VehicleManufacturer.GenerateOne();

        Bike bike = _fakers.Bike.GenerateOne();
        bike.Lights = _fakers.BicycleLight.GenerateSet(15);
        manufacturer.Vehicles.Add(bike);

        Tandem tandem = _fakers.Tandem.GenerateOne();
        tandem.Features = _fakers.GenericFeature.GenerateSet(15);
        manufacturer.Vehicles.Add(tandem);

        Car car = _fakers.Car.GenerateOne();
        car.Engine = _fakers.GasolineEngine.GenerateOne();
        car.Features = _fakers.GenericFeature.GenerateSet(15);
        manufacturer.Vehicles.Add(car);

        Truck truck = _fakers.Truck.GenerateOne();
        truck.Engine = _fakers.GasolineEngine.GenerateOne();
        truck.Features = _fakers.GenericFeature.GenerateSet(15);
        manufacturer.Vehicles.Add(truck);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.VehicleManufacturers.Add(manufacturer);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/vehicleManufacturers/{manufacturer.StringId}?include=vehicles.lights,vehicles.features";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("vehicleManufacturers");
        responseDocument.Data.SingleValue.Id.Should().Be(manufacturer.StringId);

        responseDocument.Included.Should().NotBeNull();
        responseDocument.Included.Where(include => include.Type == "bicycleLights").Should().HaveCount(10);
        responseDocument.Included.Where(include => include.Type == "genericFeatures").Should().HaveCount(10 * 3);
    }

    [Fact]
    public async Task Can_get_secondary_resource_at_abstract_base_endpoint()
    {
        // Arrange
        Car car = _fakers.Car.GenerateOne();
        car.Engine = _fakers.GasolineEngine.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(car);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/motorVehicles/{car.StringId}/engine";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Self.Should().Be(route);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("gasolineEngines");
        responseDocument.Data.SingleValue.Id.Should().Be(car.Engine.StringId);

        responseDocument.Data.SingleValue.Links.Should().NotBeNull();
        responseDocument.Data.SingleValue.Links.Self.Should().Be($"/gasolineEngines/{car.Engine.StringId}");

        responseDocument.Data.SingleValue.Attributes.Should().HaveCount(4);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("isHydrocarbonBased").WhoseValue.Should().Be(car.Engine.IsHydrocarbonBased);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("capacity").WhoseValue.Should().Be(car.Engine.Capacity);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("serialCode").WhoseValue.Should().Be(((GasolineEngine)car.Engine).SerialCode);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("volatility").WhoseValue.Should().Be(((GasolineEngine)car.Engine).Volatility);

        responseDocument.Data.SingleValue.Relationships.Should().HaveCount(1);

        responseDocument.Data.SingleValue.Relationships.Should().ContainKey("cylinders").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();
            value.Links.Should().NotBeNull();
            value.Links.Self.Should().Be($"/gasolineEngines/{car.Engine.StringId}/relationships/cylinders");
            value.Links.Related.Should().Be($"/gasolineEngines/{car.Engine.StringId}/cylinders");
        });
    }

    [Fact]
    public async Task Can_get_secondary_resource_at_concrete_base_endpoint()
    {
        // Arrange
        Tandem tandem = _fakers.Tandem.GenerateOne();
        tandem.CargoBox = _fakers.Box.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(tandem);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/bikes/{tandem.StringId}/cargoBox";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Self.Should().Be(route);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("boxes");
        responseDocument.Data.SingleValue.Id.Should().Be(tandem.CargoBox.StringId);

        responseDocument.Data.SingleValue.Links.Should().BeNull();

        responseDocument.Data.SingleValue.Attributes.Should().HaveCount(3);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("width").WhoseValue.Should().Be(tandem.CargoBox.Width);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("height").WhoseValue.Should().Be(tandem.CargoBox.Height);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("depth").WhoseValue.Should().Be(tandem.CargoBox.Depth);

        responseDocument.Data.SingleValue.Relationships.Should().BeNull();
    }

    [Fact]
    public async Task Can_get_secondary_resource_at_concrete_derived_endpoint()
    {
        // Arrange
        Car car = _fakers.Car.GenerateOne();
        car.Engine = _fakers.GasolineEngine.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(car);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/cars/{car.StringId}/engine";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Self.Should().Be(route);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("gasolineEngines");
        responseDocument.Data.SingleValue.Id.Should().Be(car.Engine.StringId);

        responseDocument.Data.SingleValue.Links.Should().NotBeNull();
        responseDocument.Data.SingleValue.Links.Self.Should().Be($"/gasolineEngines/{car.Engine.StringId}");

        responseDocument.Data.SingleValue.Attributes.Should().HaveCount(4);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("isHydrocarbonBased").WhoseValue.Should().Be(car.Engine.IsHydrocarbonBased);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("capacity").WhoseValue.Should().Be(car.Engine.Capacity);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("serialCode").WhoseValue.Should().Be(((GasolineEngine)car.Engine).SerialCode);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("volatility").WhoseValue.Should().Be(((GasolineEngine)car.Engine).Volatility);

        responseDocument.Data.SingleValue.Relationships.Should().HaveCount(1);

        responseDocument.Data.SingleValue.Relationships.Should().ContainKey("cylinders").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();
            value.Links.Should().NotBeNull();
            value.Links.Self.Should().Be($"/gasolineEngines/{car.Engine.StringId}/relationships/cylinders");
            value.Links.Related.Should().Be($"/gasolineEngines/{car.Engine.StringId}/cylinders");
        });
    }

    [Fact]
    public async Task Cannot_get_secondary_resource_defined_in_derived_type_at_abstract_endpoint()
    {
        // Arrange
        Car car = _fakers.Car.GenerateOne();
        car.Engine = _fakers.GasolineEngine.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(car);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/vehicles/{car.StringId}/engine";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested relationship does not exist.");
        error.Detail.Should().Be("Resource of type 'vehicles' does not contain a relationship named 'engine'.");
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_get_secondary_resource_defined_in_derived_type_at_concrete_base_endpoint()
    {
        // Arrange
        Tandem tandem = _fakers.Tandem.GenerateOne();
        tandem.FoldingDimensions = _fakers.Box.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(tandem);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/bikes/{tandem.StringId}/foldingDimensions";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested relationship does not exist.");
        error.Detail.Should().Be("Resource of type 'bikes' does not contain a relationship named 'foldingDimensions'.");
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Can_get_secondary_resources_at_abstract_base_endpoint()
    {
        // Arrange
        Car car = _fakers.Car.GenerateOne();
        car.Engine = _fakers.GasolineEngine.GenerateOne();
        car.Wheels = _fakers.ChromeWheel.GenerateSet<ChromeWheel, Wheel>(2).Concat(_fakers.CarbonWheel.GenerateSet<CarbonWheel, Wheel>(2)).ToHashSet();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(car);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/vehicles/{car.StringId}/wheels";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Self.Should().Be(route);

        responseDocument.Data.ManyValue.Should().HaveCount(4);

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "chromeWheels" && resource.Id == car.Wheels.ElementAt(0).StringId);
        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "chromeWheels" && resource.Id == car.Wheels.ElementAt(1).StringId);
        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "carbonWheels" && resource.Id == car.Wheels.ElementAt(2).StringId);
        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "carbonWheels" && resource.Id == car.Wheels.ElementAt(3).StringId);

        foreach (ResourceObject resource in responseDocument.Data.ManyValue.Where(value => value.Type == "chromeWheels"))
        {
            resource.Links.Should().NotBeNull();
            resource.Links.Self.Should().Be($"/chromeWheels/{resource.Id}");

            resource.Attributes.Should().OnlyContainKeys("radius", "paintColor");
        }

        foreach (ResourceObject resource in responseDocument.Data.ManyValue.Where(value => value.Type == "carbonWheels"))
        {
            resource.Links.Should().NotBeNull();
            resource.Links.Self.Should().Be($"/carbonWheels/{resource.Id}");

            resource.Attributes.Should().OnlyContainKeys("radius", "hasTube");
        }

        foreach (ResourceObject resource in responseDocument.Data.ManyValue)
        {
            resource.Relationships.Should().HaveCount(1);

            resource.Relationships.Should().ContainKey("vehicle").WhoseValue.With(value =>
            {
                value.Should().NotBeNull();
                value.Links.Should().NotBeNull();
                value.Links.Self.Should().Be($"/{resource.Type}/{resource.Id}/relationships/vehicle");
                value.Links.Related.Should().Be($"/{resource.Type}/{resource.Id}/vehicle");
            });
        }
    }

    [Fact]
    public async Task Can_get_secondary_resources_at_concrete_base_endpoint()
    {
        // Arrange
        Tandem tandem = _fakers.Tandem.GenerateOne();
        tandem.Wheels = _fakers.ChromeWheel.GenerateSet<ChromeWheel, Wheel>(2).Concat(_fakers.CarbonWheel.GenerateSet<CarbonWheel, Wheel>(2)).ToHashSet();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(tandem);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/bikes/{tandem.StringId}/wheels";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Self.Should().Be(route);

        responseDocument.Data.ManyValue.Should().HaveCount(4);

        responseDocument.Data.ManyValue.Should()
            .ContainSingle(resource => resource.Type == "chromeWheels" && resource.Id == tandem.Wheels.ElementAt(0).StringId);

        responseDocument.Data.ManyValue.Should()
            .ContainSingle(resource => resource.Type == "chromeWheels" && resource.Id == tandem.Wheels.ElementAt(1).StringId);

        responseDocument.Data.ManyValue.Should()
            .ContainSingle(resource => resource.Type == "carbonWheels" && resource.Id == tandem.Wheels.ElementAt(2).StringId);

        responseDocument.Data.ManyValue.Should()
            .ContainSingle(resource => resource.Type == "carbonWheels" && resource.Id == tandem.Wheels.ElementAt(3).StringId);

        foreach (ResourceObject resource in responseDocument.Data.ManyValue.Where(value => value.Type == "chromeWheels"))
        {
            resource.Links.Should().NotBeNull();
            resource.Links.Self.Should().Be($"/chromeWheels/{resource.Id}");

            resource.Attributes.Should().OnlyContainKeys("radius", "paintColor");
        }

        foreach (ResourceObject resource in responseDocument.Data.ManyValue.Where(value => value.Type == "carbonWheels"))
        {
            resource.Links.Should().NotBeNull();
            resource.Links.Self.Should().Be($"/carbonWheels/{resource.Id}");

            resource.Attributes.Should().OnlyContainKeys("radius", "hasTube");
        }

        foreach (ResourceObject resource in responseDocument.Data.ManyValue)
        {
            resource.Relationships.Should().HaveCount(1);

            resource.Relationships.Should().ContainKey("vehicle").WhoseValue.With(value =>
            {
                value.Should().NotBeNull();
                value.Links.Should().NotBeNull();
                value.Links.Self.Should().Be($"/{resource.Type}/{resource.Id}/relationships/vehicle");
                value.Links.Related.Should().Be($"/{resource.Type}/{resource.Id}/vehicle");
            });
        }
    }

    [Fact]
    public async Task Can_get_secondary_resources_at_concrete_derived_endpoint()
    {
        // Arrange
        Car car = _fakers.Car.GenerateOne();
        car.Engine = _fakers.GasolineEngine.GenerateOne();
        car.Wheels = _fakers.ChromeWheel.GenerateSet<ChromeWheel, Wheel>(2).Concat(_fakers.CarbonWheel.GenerateSet<CarbonWheel, Wheel>(2)).ToHashSet();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(car);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/cars/{car.StringId}/wheels";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Self.Should().Be(route);

        responseDocument.Data.ManyValue.Should().HaveCount(4);

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "chromeWheels" && resource.Id == car.Wheels.ElementAt(0).StringId);
        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "chromeWheels" && resource.Id == car.Wheels.ElementAt(1).StringId);
        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "carbonWheels" && resource.Id == car.Wheels.ElementAt(2).StringId);
        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "carbonWheels" && resource.Id == car.Wheels.ElementAt(3).StringId);

        foreach (ResourceObject resource in responseDocument.Data.ManyValue.Where(value => value.Type == "chromeWheels"))
        {
            resource.Links.Should().NotBeNull();
            resource.Links.Self.Should().Be($"/chromeWheels/{resource.Id}");

            resource.Attributes.Should().OnlyContainKeys("radius", "paintColor");
        }

        foreach (ResourceObject resource in responseDocument.Data.ManyValue.Where(value => value.Type == "carbonWheels"))
        {
            resource.Links.Should().NotBeNull();
            resource.Links.Self.Should().Be($"/carbonWheels/{resource.Id}");

            resource.Attributes.Should().OnlyContainKeys("radius", "hasTube");
        }

        foreach (ResourceObject resource in responseDocument.Data.ManyValue)
        {
            resource.Relationships.Should().HaveCount(1);

            resource.Relationships.Should().ContainKey("vehicle").WhoseValue.With(value =>
            {
                value.Should().NotBeNull();
                value.Links.Should().NotBeNull();
                value.Links.Self.Should().Be($"/{resource.Type}/{resource.Id}/relationships/vehicle");
                value.Links.Related.Should().Be($"/{resource.Type}/{resource.Id}/vehicle");
            });
        }
    }

    [Fact]
    public async Task Cannot_get_secondary_resources_defined_in_derived_type_at_abstract_endpoint()
    {
        // Arrange
        Truck truck = _fakers.Truck.GenerateOne();
        truck.Engine = _fakers.DieselEngine.GenerateOne();
        truck.Features = _fakers.GenericFeature.GenerateSet(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(truck);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/vehicles/{truck.StringId}/features";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested relationship does not exist.");
        error.Detail.Should().Be("Resource of type 'vehicles' does not contain a relationship named 'features'.");
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_get_secondary_resources_defined_in_derived_type_at_concrete_base_endpoint()
    {
        // Arrange
        Tandem tandem = _fakers.Tandem.GenerateOne();
        tandem.Features = _fakers.GenericFeature.GenerateSet(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(tandem);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/bikes/{tandem.StringId}/features";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested relationship does not exist.");
        error.Detail.Should().Be("Resource of type 'bikes' does not contain a relationship named 'features'.");
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Can_get_ToOne_relationship_at_abstract_base_endpoint()
    {
        // Arrange
        Car car = _fakers.Car.GenerateOne();
        car.Engine = _fakers.GasolineEngine.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(car);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/motorVehicles/{car.StringId}/relationships/engine";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Self.Should().Be(route);
        responseDocument.Links.Related.Should().Be($"/motorVehicles/{car.StringId}/engine");

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("gasolineEngines");
        responseDocument.Data.SingleValue.Id.Should().Be(car.Engine.StringId);
        responseDocument.Data.SingleValue.Links.Should().BeNull();
    }

    [Fact]
    public async Task Can_get_ToOne_relationship_at_concrete_base_endpoint()
    {
        // Arrange
        Tandem tandem = _fakers.Tandem.GenerateOne();
        tandem.CargoBox = _fakers.Box.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(tandem);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/bikes/{tandem.StringId}/relationships/cargoBox";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Self.Should().Be(route);
        responseDocument.Links.Related.Should().Be($"/bikes/{tandem.StringId}/cargoBox");

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("boxes");
        responseDocument.Data.SingleValue.Id.Should().Be(tandem.CargoBox.StringId);
        responseDocument.Data.SingleValue.Links.Should().BeNull();
    }

    [Fact]
    public async Task Can_get_ToOne_relationship_at_concrete_derived_endpoint()
    {
        // Arrange
        Car car = _fakers.Car.GenerateOne();
        car.Engine = _fakers.GasolineEngine.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(car);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/cars/{car.StringId}/relationships/engine";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Self.Should().Be(route);
        responseDocument.Links.Related.Should().Be($"/cars/{car.StringId}/engine");

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("gasolineEngines");
        responseDocument.Data.SingleValue.Id.Should().Be(car.Engine.StringId);
        responseDocument.Data.SingleValue.Links.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_get_ToOne_relationship_defined_in_derived_type_at_abstract_endpoint()
    {
        // Arrange
        Car car = _fakers.Car.GenerateOne();
        car.Engine = _fakers.GasolineEngine.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(car);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/vehicles/{car.StringId}/relationships/engine";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested relationship does not exist.");
        error.Detail.Should().Be("Resource of type 'vehicles' does not contain a relationship named 'engine'.");
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Can_get_ToMany_relationship_at_abstract_base_endpoint()
    {
        // Arrange
        Car car = _fakers.Car.GenerateOne();
        car.Engine = _fakers.GasolineEngine.GenerateOne();
        car.Wheels = _fakers.ChromeWheel.GenerateSet<ChromeWheel, Wheel>(2).Concat(_fakers.CarbonWheel.GenerateSet<CarbonWheel, Wheel>(2)).ToHashSet();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(car);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/vehicles/{car.StringId}/relationships/wheels";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Self.Should().Be(route);
        responseDocument.Links.Related.Should().Be($"/vehicles/{car.StringId}/wheels");

        responseDocument.Data.ManyValue.Should().HaveCount(4);

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "chromeWheels" && resource.Id == car.Wheels.ElementAt(0).StringId);
        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "chromeWheels" && resource.Id == car.Wheels.ElementAt(1).StringId);
        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "carbonWheels" && resource.Id == car.Wheels.ElementAt(2).StringId);
        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "carbonWheels" && resource.Id == car.Wheels.ElementAt(3).StringId);

        foreach (ResourceObject resource in responseDocument.Data.ManyValue)
        {
            resource.Links.Should().BeNull();
        }
    }

    [Fact]
    public async Task Can_get_ToMany_relationship_at_concrete_base_endpoint()
    {
        // Arrange
        Tandem tandem = _fakers.Tandem.GenerateOne();
        tandem.Wheels = _fakers.ChromeWheel.GenerateSet<ChromeWheel, Wheel>(2).Concat(_fakers.CarbonWheel.GenerateSet<CarbonWheel, Wheel>(2)).ToHashSet();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(tandem);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/bikes/{tandem.StringId}/relationships/wheels";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Self.Should().Be(route);
        responseDocument.Links.Related.Should().Be($"/bikes/{tandem.StringId}/wheels");

        responseDocument.Data.ManyValue.Should().HaveCount(4);

        responseDocument.Data.ManyValue.Should()
            .ContainSingle(resource => resource.Type == "chromeWheels" && resource.Id == tandem.Wheels.ElementAt(0).StringId);

        responseDocument.Data.ManyValue.Should()
            .ContainSingle(resource => resource.Type == "chromeWheels" && resource.Id == tandem.Wheels.ElementAt(1).StringId);

        responseDocument.Data.ManyValue.Should()
            .ContainSingle(resource => resource.Type == "carbonWheels" && resource.Id == tandem.Wheels.ElementAt(2).StringId);

        responseDocument.Data.ManyValue.Should()
            .ContainSingle(resource => resource.Type == "carbonWheels" && resource.Id == tandem.Wheels.ElementAt(3).StringId);

        foreach (ResourceObject resource in responseDocument.Data.ManyValue)
        {
            resource.Links.Should().BeNull();
        }
    }

    [Fact]
    public async Task Can_get_ToMany_relationship_at_concrete_derived_endpoint()
    {
        // Arrange
        Car car = _fakers.Car.GenerateOne();
        car.Engine = _fakers.GasolineEngine.GenerateOne();
        car.Wheels = _fakers.ChromeWheel.GenerateSet<ChromeWheel, Wheel>(2).Concat(_fakers.CarbonWheel.GenerateSet<CarbonWheel, Wheel>(2)).ToHashSet();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(car);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/cars/{car.StringId}/relationships/wheels";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Self.Should().Be(route);
        responseDocument.Links.Related.Should().Be($"/cars/{car.StringId}/wheels");

        responseDocument.Data.ManyValue.Should().HaveCount(4);

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "chromeWheels" && resource.Id == car.Wheels.ElementAt(0).StringId);
        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "chromeWheels" && resource.Id == car.Wheels.ElementAt(1).StringId);
        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "carbonWheels" && resource.Id == car.Wheels.ElementAt(2).StringId);
        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "carbonWheels" && resource.Id == car.Wheels.ElementAt(3).StringId);

        foreach (ResourceObject resource in responseDocument.Data.ManyValue)
        {
            resource.Links.Should().BeNull();
        }
    }

    [Fact]
    public async Task Cannot_get_ToMany_relationship_defined_in_derived_type_at_abstract_endpoint()
    {
        // Arrange
        Truck truck = _fakers.Truck.GenerateOne();
        truck.Engine = _fakers.DieselEngine.GenerateOne();
        truck.Features = _fakers.GenericFeature.GenerateSet(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(truck);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/vehicles/{truck.StringId}/relationships/features";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested relationship does not exist.");
        error.Detail.Should().Be("Resource of type 'vehicles' does not contain a relationship named 'features'.");
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_get_ToMany_relationship_defined_in_derived_type_at_concrete_base_endpoint()
    {
        // Arrange
        Tandem tandem = _fakers.Tandem.GenerateOne();
        tandem.Features = _fakers.GenericFeature.GenerateSet(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(tandem);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/bikes/{tandem.StringId}/relationships/features";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested relationship does not exist.");
        error.Detail.Should().Be("Resource of type 'bikes' does not contain a relationship named 'features'.");
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Can_get_primary_resources_at_abstract_endpoint_with_all_sparse_fieldsets()
    {
        // Arrange
        Bike bike = _fakers.Bike.GenerateOne();

        Tandem tandem = _fakers.Tandem.GenerateOne();

        Car car = _fakers.Car.GenerateOne();
        car.Engine = _fakers.GasolineEngine.GenerateOne();

        Truck truck = _fakers.Truck.GenerateOne();
        truck.Engine = _fakers.DieselEngine.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Vehicle>();
            dbContext.Vehicles.AddRange(bike, tandem, car, truck);
            await dbContext.SaveChangesAsync();
        });

        const string bikeFields = "fields[bikes]=weight,gearCount,lights";
        const string tandemFields = "fields[tandems]=gearCount,passengerCount,cargoBox";
        const string carFields = "fields[cars]=weight,requiresDriverLicense,seatCount,engine";
        const string truckFields = "fields[trucks]=loadingCapacity,sleepingArea";

        const string route = $"/vehicles?{bikeFields}&{tandemFields}&{carFields}&{truckFields}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(4);

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "bikes").Subject.With(resource =>
        {
            resource.Id.Should().Be(bike.StringId);
            resource.Attributes.Should().OnlyContainKeys("weight", "gearCount");
            resource.Relationships.Should().OnlyContainKeys("lights");
        });

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "tandems").Subject.With(resource =>
        {
            resource.Id.Should().Be(tandem.StringId);
            resource.Attributes.Should().OnlyContainKeys("gearCount", "passengerCount");
            resource.Relationships.Should().OnlyContainKeys("cargoBox");
        });

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "cars").Subject.With(resource =>
        {
            resource.Id.Should().Be(car.StringId);
            resource.Attributes.Should().OnlyContainKeys("weight", "requiresDriverLicense", "seatCount");
            resource.Relationships.Should().OnlyContainKeys("engine");
        });

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "trucks").Subject.With(resource =>
        {
            resource.Id.Should().Be(truck.StringId);
            resource.Attributes.Should().OnlyContainKeys("loadingCapacity");
            resource.Relationships.Should().OnlyContainKeys("sleepingArea");
        });
    }

    [Fact]
    public async Task Can_get_primary_resources_at_abstract_endpoint_with_some_sparse_fieldsets()
    {
        // Arrange
        Bike bike = _fakers.Bike.GenerateOne();

        Tandem tandem = _fakers.Tandem.GenerateOne();

        Car car = _fakers.Car.GenerateOne();
        car.Engine = _fakers.GasolineEngine.GenerateOne();

        Truck truck = _fakers.Truck.GenerateOne();
        truck.Engine = _fakers.DieselEngine.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Vehicle>();
            dbContext.Vehicles.AddRange(bike, tandem, car, truck);
            await dbContext.SaveChangesAsync();
        });

        const string bikeFields = "fields[bikes]=weight,gearCount,lights";
        const string carFields = "fields[cars]=weight,requiresDriverLicense,seatCount,engine";

        const string route = $"/vehicles?{bikeFields}&{carFields}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(4);

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "bikes").Subject.With(resource =>
        {
            resource.Id.Should().Be(bike.StringId);
            resource.Attributes.Should().OnlyContainKeys("weight", "gearCount");
            resource.Relationships.Should().OnlyContainKeys("lights");
        });

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "tandems").Subject.With(resource =>
        {
            resource.Id.Should().Be(tandem.StringId);
            resource.Attributes.Should().OnlyContainKeys("weight", "requiresDriverLicense", "gearCount", "passengerCount");
            resource.Relationships.Should().OnlyContainKeys("manufacturer", "wheels", "lights", "cargoBox", "foldingDimensions", "features");
        });

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "cars").Subject.With(resource =>
        {
            resource.Id.Should().Be(car.StringId);
            resource.Attributes.Should().OnlyContainKeys("weight", "requiresDriverLicense", "seatCount");
            resource.Relationships.Should().OnlyContainKeys("engine");
        });

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "trucks").Subject.With(resource =>
        {
            resource.Id.Should().Be(truck.StringId);
            resource.Attributes.Should().OnlyContainKeys("weight", "requiresDriverLicense", "licensePlate", "loadingCapacity");
            resource.Relationships.Should().OnlyContainKeys("manufacturer", "wheels", "engine", "navigationSystem", "sleepingArea", "features");
        });
    }

    [Fact]
    public async Task Can_get_primary_resources_at_concrete_base_endpoint_with_all_sparse_fieldsets()
    {
        // Arrange
        Bike bike = _fakers.Bike.GenerateOne();
        Tandem tandem = _fakers.Tandem.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Vehicle>();
            dbContext.Vehicles.AddRange(bike, tandem);
            await dbContext.SaveChangesAsync();
        });

        const string bikeFields = "fields[bikes]=weight,gearCount,manufacturer,lights";
        const string tandemFields = "fields[tandems]=passengerCount,cargoBox";

        const string route = $"/bikes?{bikeFields}&{tandemFields}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "bikes").Subject.With(resource =>
        {
            resource.Id.Should().Be(bike.StringId);

            resource.Attributes.Should().OnlyContainKeys("weight", "gearCount");
            resource.Relationships.Should().OnlyContainKeys("manufacturer", "lights");
        });

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "tandems").Subject.With(resource =>
        {
            resource.Id.Should().Be(tandem.StringId);

            resource.Attributes.Should().OnlyContainKeys("passengerCount");
            resource.Relationships.Should().OnlyContainKeys("cargoBox");
        });
    }

    [Fact]
    public async Task Can_get_primary_resources_at_concrete_base_endpoint_with_some_sparse_fieldsets()
    {
        // Arrange
        Bike bike = _fakers.Bike.GenerateOne();
        Tandem tandem = _fakers.Tandem.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Vehicle>();
            dbContext.Vehicles.AddRange(bike, tandem);
            await dbContext.SaveChangesAsync();
        });

        const string tandemFields = "fields[tandems]=passengerCount,cargoBox";

        const string route = $"/bikes?{tandemFields}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "bikes").Subject.With(resource =>
        {
            resource.Id.Should().Be(bike.StringId);

            resource.Attributes.Should().OnlyContainKeys("weight", "requiresDriverLicense", "gearCount");
            resource.Relationships.Should().OnlyContainKeys("manufacturer", "wheels", "cargoBox", "lights");
        });

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "tandems").Subject.With(resource =>
        {
            resource.Id.Should().Be(tandem.StringId);

            resource.Attributes.Should().OnlyContainKeys("passengerCount");
            resource.Relationships.Should().OnlyContainKeys("cargoBox");
        });
    }

    [Fact]
    public async Task Can_get_primary_resources_at_abstract_endpoint_with_derived_includes()
    {
        // Arrange
        Bike bike = _fakers.Bike.GenerateOne();
        bike.Manufacturer = _fakers.VehicleManufacturer.GenerateOne();
        bike.Wheels = _fakers.CarbonWheel.GenerateSet<CarbonWheel, Wheel>(1);
        bike.CargoBox = _fakers.Box.GenerateOne();
        bike.Lights = _fakers.BicycleLight.GenerateSet(1);

        Tandem tandem = _fakers.Tandem.GenerateOne();
        tandem.Manufacturer = _fakers.VehicleManufacturer.GenerateOne();
        tandem.Wheels = _fakers.ChromeWheel.GenerateSet<ChromeWheel, Wheel>(1);
        tandem.CargoBox = _fakers.Box.GenerateOne();
        tandem.Lights = _fakers.BicycleLight.GenerateSet(1);
        tandem.Features = _fakers.GenericFeature.GenerateSet(1);
        tandem.Features.ElementAt(0).Properties = _fakers.StringProperty.GenerateSet<StringProperty, GenericProperty>(1);
        ((StringProperty)tandem.Features.ElementAt(0).Properties.ElementAt(0)).Value = _fakers.StringValue.GenerateOne();

        Car car = _fakers.Car.GenerateOne();
        car.Manufacturer = _fakers.VehicleManufacturer.GenerateOne();
        car.Wheels = _fakers.CarbonWheel.GenerateSet<CarbonWheel, Wheel>(1);
        car.Engine = _fakers.GasolineEngine.GenerateOne();
        ((GasolineEngine)car.Engine).Cylinders = _fakers.Cylinder.GenerateSet(1);
        car.NavigationSystem = _fakers.NavigationSystem.GenerateOne();
        car.Features = _fakers.GenericFeature.GenerateSet(1);
        car.Features.ElementAt(0).Properties = _fakers.NumberProperty.GenerateSet<NumberProperty, GenericProperty>(1);
        ((NumberProperty)car.Features.ElementAt(0).Properties.ElementAt(0)).Value = _fakers.NumberValue.GenerateOne();

        Truck truck = _fakers.Truck.GenerateOne();
        truck.Manufacturer = _fakers.VehicleManufacturer.GenerateOne();
        truck.Wheels = _fakers.ChromeWheel.GenerateSet<ChromeWheel, Wheel>(1);
        truck.Engine = _fakers.DieselEngine.GenerateOne();
        truck.NavigationSystem = _fakers.NavigationSystem.GenerateOne();
        truck.SleepingArea = _fakers.Box.GenerateOne();
        truck.Features = _fakers.GenericFeature.GenerateSet(1);
        truck.Features.ElementAt(0).Properties = _fakers.StringProperty.GenerateSet<StringProperty, GenericProperty>(1);
        ((StringProperty)truck.Features.ElementAt(0).Properties.ElementAt(0)).Value = _fakers.StringValue.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Vehicle>();
            dbContext.Vehicles.AddRange(bike, tandem, car, truck);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/vehicles?include=manufacturer,wheels,cargoBox,lights,features.properties.value,engine.cylinders,navigationSystem,sleepingArea";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Should().BeJson($$"""
            {
              "links": {
                "self": "{{route}}",
                "first": "{{route}}"
              },
              "data": [
                {
                  "type": "bikes",
                  "id": "{{bike.StringId}}",
                  "attributes": {
                    "requiresDriverLicense": {{bike.RequiresDriverLicense.ToString().ToLowerInvariant()}},
                    "gearCount": {{bike.GearCount}},
                    "weight": {{bike.Weight.ToString(CultureInfo.InvariantCulture)}}
                  },
                  "relationships": {
                    "cargoBox": {
                      "links": {
                        "self": "/bikes/{{bike.StringId}}/relationships/cargoBox",
                        "related": "/bikes/{{bike.StringId}}/cargoBox"
                      },
                      "data": {
                        "type": "boxes",
                        "id": "{{bike.CargoBox.StringId}}"
                      }
                    },
                    "lights": {
                      "links": {
                        "self": "/bikes/{{bike.StringId}}/relationships/lights",
                        "related": "/bikes/{{bike.StringId}}/lights"
                      },
                      "data": [
                        {
                          "type": "bicycleLights",
                          "id": "{{bike.Lights.ElementAt(0).StringId}}"
                        }
                      ]
                    },
                    "manufacturer": {
                      "links": {
                        "self": "/bikes/{{bike.StringId}}/relationships/manufacturer",
                        "related": "/bikes/{{bike.StringId}}/manufacturer"
                      },
                      "data": {
                        "type": "vehicleManufacturers",
                        "id": "{{bike.Manufacturer.StringId}}"
                      }
                    },
                    "wheels": {
                      "links": {
                        "self": "/bikes/{{bike.StringId}}/relationships/wheels",
                        "related": "/bikes/{{bike.StringId}}/wheels"
                      },
                      "data": [
                        {
                          "type": "carbonWheels",
                          "id": "{{bike.Wheels.OfType<CarbonWheel>().ElementAt(0).StringId}}"
                        }
                      ]
                    }
                  },
                  "links": {
                    "self": "/bikes/{{bike.StringId}}"
                  }
                },
                {
                  "type": "cars",
                  "id": "{{car.StringId}}",
                  "attributes": {
                    "seatCount": {{car.SeatCount}},
                    "requiresDriverLicense": {{car.RequiresDriverLicense.ToString().ToLowerInvariant()}},
                    "licensePlate": "{{car.LicensePlate}}",
                    "weight": {{car.Weight.ToString(CultureInfo.InvariantCulture)}}
                  },
                  "relationships": {
                    "features": {
                      "links": {
                        "self": "/cars/{{car.StringId}}/relationships/features",
                        "related": "/cars/{{car.StringId}}/features"
                      },
                      "data": [
                        {
                          "type": "genericFeatures",
                          "id": "{{car.Features.ElementAt(0).StringId}}"
                        }
                      ]
                    },
                    "engine": {
                      "links": {
                        "self": "/cars/{{car.StringId}}/relationships/engine",
                        "related": "/cars/{{car.StringId}}/engine"
                      },
                      "data": {
                        "type": "gasolineEngines",
                        "id": "{{car.Engine.StringId}}"
                      }
                    },
                    "navigationSystem": {
                      "links": {
                        "self": "/cars/{{car.StringId}}/relationships/navigationSystem",
                        "related": "/cars/{{car.StringId}}/navigationSystem"
                      },
                      "data": {
                        "type": "navigationSystems",
                        "id": "{{car.NavigationSystem.StringId}}"
                      }
                    },
                    "manufacturer": {
                      "links": {
                        "self": "/cars/{{car.StringId}}/relationships/manufacturer",
                        "related": "/cars/{{car.StringId}}/manufacturer"
                      },
                      "data": {
                        "type": "vehicleManufacturers",
                        "id": "{{car.Manufacturer.StringId}}"
                      }
                    },
                    "wheels": {
                      "links": {
                        "self": "/cars/{{car.StringId}}/relationships/wheels",
                        "related": "/cars/{{car.StringId}}/wheels"
                      },
                      "data": [
                        {
                          "type": "carbonWheels",
                          "id": "{{car.Wheels.OfType<CarbonWheel>().ElementAt(0).StringId}}"
                        }
                      ]
                    }
                  },
                  "links": {
                    "self": "/cars/{{car.StringId}}"
                  }
                },
                {
                  "type": "tandems",
                  "id": "{{tandem.StringId}}",
                  "attributes": {
                    "passengerCount": {{tandem.PassengerCount}},
                    "requiresDriverLicense": {{tandem.RequiresDriverLicense.ToString().ToLowerInvariant()}},
                    "gearCount": {{tandem.GearCount}},
                    "weight": {{tandem.Weight.ToString(CultureInfo.InvariantCulture)}}
                  },
                  "relationships": {
                    "foldingDimensions": {
                      "links": {
                        "self": "/tandems/{{tandem.StringId}}/relationships/foldingDimensions",
                        "related": "/tandems/{{tandem.StringId}}/foldingDimensions"
                      }
                    },
                    "features": {
                      "links": {
                        "self": "/tandems/{{tandem.StringId}}/relationships/features",
                        "related": "/tandems/{{tandem.StringId}}/features"
                      },
                      "data": [
                        {
                          "type": "genericFeatures",
                          "id": "{{tandem.Features.ElementAt(0).StringId}}"
                        }
                      ]
                    },
                    "cargoBox": {
                      "links": {
                        "self": "/tandems/{{tandem.StringId}}/relationships/cargoBox",
                        "related": "/tandems/{{tandem.StringId}}/cargoBox"
                      },
                      "data": {
                        "type": "boxes",
                        "id": "{{tandem.CargoBox.StringId}}"
                      }
                    },
                    "lights": {
                      "links": {
                        "self": "/tandems/{{tandem.StringId}}/relationships/lights",
                        "related": "/tandems/{{tandem.StringId}}/lights"
                      },
                      "data": [
                        {
                          "type": "bicycleLights",
                          "id": "{{tandem.Lights.ElementAt(0).StringId}}"
                        }
                      ]
                    },
                    "manufacturer": {
                      "links": {
                        "self": "/tandems/{{tandem.StringId}}/relationships/manufacturer",
                        "related": "/tandems/{{tandem.StringId}}/manufacturer"
                      },
                      "data": {
                        "type": "vehicleManufacturers",
                        "id": "{{tandem.Manufacturer.StringId}}"
                      }
                    },
                    "wheels": {
                      "links": {
                        "self": "/tandems/{{tandem.StringId}}/relationships/wheels",
                        "related": "/tandems/{{tandem.StringId}}/wheels"
                      },
                      "data": [
                        {
                          "type": "chromeWheels",
                          "id": "{{tandem.Wheels.ElementAt(0).StringId}}"
                        }
                      ]
                    }
                  },
                  "links": {
                    "self": "/tandems/{{tandem.StringId}}"
                  }
                },
                {
                  "type": "trucks",
                  "id": "{{truck.StringId}}",
                  "attributes": {
                    "loadingCapacity": {{truck.LoadingCapacity.ToString(CultureInfo.InvariantCulture)}},
                    "requiresDriverLicense": {{truck.RequiresDriverLicense.ToString().ToLowerInvariant()}},
                    "licensePlate": "{{truck.LicensePlate}}",
                    "weight": {{truck.Weight.ToString(CultureInfo.InvariantCulture)}}
                  },
                  "relationships": {
                    "sleepingArea": {
                      "links": {
                        "self": "/trucks/{{truck.StringId}}/relationships/sleepingArea",
                        "related": "/trucks/{{truck.StringId}}/sleepingArea"
                      },
                      "data": {
                        "type": "boxes",
                        "id": "{{truck.SleepingArea.StringId}}"
                      }
                    },
                    "features": {
                      "links": {
                        "self": "/trucks/{{truck.StringId}}/relationships/features",
                        "related": "/trucks/{{truck.StringId}}/features"
                      },
                      "data": [
                        {
                          "type": "genericFeatures",
                          "id": "{{truck.Features.ElementAt(0).StringId}}"
                        }
                      ]
                    },
                    "engine": {
                      "links": {
                        "self": "/trucks/{{truck.StringId}}/relationships/engine",
                        "related": "/trucks/{{truck.StringId}}/engine"
                      },
                      "data": {
                        "type": "dieselEngines",
                        "id": "{{truck.Engine.StringId}}"
                      }
                    },
                    "navigationSystem": {
                      "links": {
                        "self": "/trucks/{{truck.StringId}}/relationships/navigationSystem",
                        "related": "/trucks/{{truck.StringId}}/navigationSystem"
                      },
                      "data": {
                        "type": "navigationSystems",
                        "id": "{{truck.NavigationSystem.StringId}}"
                      }
                    },
                    "manufacturer": {
                      "links": {
                        "self": "/trucks/{{truck.StringId}}/relationships/manufacturer",
                        "related": "/trucks/{{truck.StringId}}/manufacturer"
                      },
                      "data": {
                        "type": "vehicleManufacturers",
                        "id": "{{truck.Manufacturer.StringId}}"
                      }
                    },
                    "wheels": {
                      "links": {
                        "self": "/trucks/{{truck.StringId}}/relationships/wheels",
                        "related": "/trucks/{{truck.StringId}}/wheels"
                      },
                      "data": [
                        {
                          "type": "chromeWheels",
                          "id": "{{truck.Wheels.ElementAt(0).StringId}}"
                        }
                      ]
                    }
                  },
                  "links": {
                    "self": "/trucks/{{truck.StringId}}"
                  }
                }
              ],
              "included": [
                {
                  "type": "boxes",
                  "id": "{{bike.CargoBox.StringId}}",
                  "attributes": {
                    "width": {{bike.CargoBox.Width.ToString(CultureInfo.InvariantCulture)}},
                    "height": {{bike.CargoBox.Height.ToString(CultureInfo.InvariantCulture)}},
                    "depth": {{bike.CargoBox.Depth.ToString(CultureInfo.InvariantCulture)}}
                  }
                },
                {
                  "type": "bicycleLights",
                  "id": "{{bike.Lights.ElementAt(0).StringId}}",
                  "attributes": {
                    "color": "{{bike.Lights.ElementAt(0).Color}}"
                  }
                },
                {
                  "type": "vehicleManufacturers",
                  "id": "{{bike.Manufacturer.StringId}}",
                  "attributes": {
                    "name": "{{bike.Manufacturer.Name}}"
                  },
                  "relationships": {
                    "vehicles": {
                      "links": {
                        "self": "/vehicleManufacturers/{{bike.Manufacturer.StringId}}/relationships/vehicles",
                        "related": "/vehicleManufacturers/{{bike.Manufacturer.StringId}}/vehicles"
                      }
                    }
                  },
                  "links": {
                    "self": "/vehicleManufacturers/{{bike.Manufacturer.StringId}}"
                  }
                },
                {
                  "type": "carbonWheels",
                  "id": "{{bike.Wheels.ElementAt(0).StringId}}",
                  "attributes": {
                    "hasTube": {{bike.Wheels.Cast<CarbonWheel>().ElementAt(0).HasTube.ToString().ToLowerInvariant()}},
                    "radius": {{bike.Wheels.ElementAt(0).Radius.ToString(CultureInfo.InvariantCulture)}}
                  },
                  "relationships": {
                    "vehicle": {
                      "links": {
                        "self": "/carbonWheels/{{bike.Wheels.ElementAt(0).StringId}}/relationships/vehicle",
                        "related": "/carbonWheels/{{bike.Wheels.ElementAt(0).StringId}}/vehicle"
                      }
                    }
                  },
                  "links": {
                    "self": "/carbonWheels/{{bike.Wheels.ElementAt(0).StringId}}"
                  }
                },
                {
                  "type": "genericFeatures",
                  "id": "{{car.Features.ElementAt(0).StringId}}",
                  "attributes": {
                    "description": "{{car.Features.ElementAt(0).Description}}"
                  },
                  "relationships": {
                    "properties": {
                      "data": [
                        {
                          "type": "numberProperties",
                          "id": "{{car.Features.ElementAt(0).Properties.ElementAt(0).StringId}}"
                        }
                      ]
                    }
                  }
                },
                {
                  "type": "numberProperties",
                  "id": "{{car.Features.ElementAt(0).Properties.ElementAt(0).StringId}}",
                  "attributes": {
                    "name": "{{car.Features.ElementAt(0).Properties.ElementAt(0).Name}}"
                  },
                  "relationships": {
                    "value": {
                      "data": {
                        "type": "numberValues",
                        "id": "{{car.Features.ElementAt(0).Properties.OfType<NumberProperty>().ElementAt(0).Value.StringId}}"
                      }
                    }
                  }
                },
                {
                  "type": "numberValues",
                  "id": "{{car.Features.ElementAt(0).Properties.OfType<NumberProperty>().ElementAt(0).Value.StringId}}",
                  "attributes": {
                    "content": {{car.Features.ElementAt(0).Properties.OfType<NumberProperty>().ElementAt(0).Value.Content.ToString(CultureInfo.InvariantCulture)}}
                  }
                },
                {
                  "type": "gasolineEngines",
                  "id": "{{car.Engine.StringId}}",
                  "attributes": {
                    "isHydrocarbonBased": {{car.Engine.IsHydrocarbonBased.ToString().ToLowerInvariant()}},
                    "serialCode": "{{((GasolineEngine)car.Engine).SerialCode}}",
                    "volatility": {{((GasolineEngine)car.Engine).Volatility.ToString(CultureInfo.InvariantCulture)}},
                    "capacity": {{car.Engine.Capacity.ToString(CultureInfo.InvariantCulture)}}
                  },
                  "relationships": {
                    "cylinders": {
                      "links": {
                        "self": "/gasolineEngines/{{car.Engine.StringId}}/relationships/cylinders",
                        "related": "/gasolineEngines/{{car.Engine.StringId}}/cylinders"
                      },
                      "data": [
                        {
                          "type": "cylinders",
                          "id": "{{((GasolineEngine)car.Engine).Cylinders.ElementAt(0).StringId}}"
                        }
                      ]
                    }
                  },
                  "links": {
                    "self": "/gasolineEngines/{{car.Engine.StringId}}"
                  }
                },
                {
                  "type": "cylinders",
                  "id": "{{((GasolineEngine)car.Engine).Cylinders.ElementAt(0).StringId}}",
                  "attributes": {
                    "sparkPlugCount": {{((GasolineEngine)car.Engine).Cylinders.ElementAt(0).SparkPlugCount}}
                  }
                },
                {
                  "type": "navigationSystems",
                  "id": "{{car.NavigationSystem.StringId}}",
                  "attributes": {
                    "modelType": "{{car.NavigationSystem.ModelType}}"
                  }
                },
                {
                  "type": "vehicleManufacturers",
                  "id": "{{car.Manufacturer.StringId}}",
                  "attributes": {
                    "name": "{{car.Manufacturer.Name}}"
                  },
                  "relationships": {
                    "vehicles": {
                      "links": {
                        "self": "/vehicleManufacturers/{{car.Manufacturer.StringId}}/relationships/vehicles",
                        "related": "/vehicleManufacturers/{{car.Manufacturer.StringId}}/vehicles"
                      }
                    }
                  },
                  "links": {
                    "self": "/vehicleManufacturers/{{car.Manufacturer.StringId}}"
                  }
                },
                {
                  "type": "carbonWheels",
                  "id": "{{car.Wheels.ElementAt(0).StringId}}",
                  "attributes": {
                    "hasTube": {{car.Wheels.OfType<CarbonWheel>().ElementAt(0).HasTube.ToString().ToLowerInvariant()}},
                    "radius": {{car.Wheels.ElementAt(0).Radius.ToString(CultureInfo.InvariantCulture)}}
                  },
                  "relationships": {
                    "vehicle": {
                      "links": {
                        "self": "/carbonWheels/{{car.Wheels.ElementAt(0).StringId}}/relationships/vehicle",
                        "related": "/carbonWheels/{{car.Wheels.ElementAt(0).StringId}}/vehicle"
                      }
                    }
                  },
                  "links": {
                    "self": "/carbonWheels/{{car.Wheels.ElementAt(0).StringId}}"
                  }
                },
                {
                  "type": "genericFeatures",
                  "id": "{{tandem.Features.ElementAt(0).StringId}}",
                  "attributes": {
                    "description": "{{tandem.Features.ElementAt(0).Description}}"
                  },
                  "relationships": {
                    "properties": {
                      "data": [
                        {
                          "type": "stringProperties",
                          "id": "{{tandem.Features.ElementAt(0).Properties.ElementAt(0).StringId}}"
                        }
                      ]
                    }
                  }
                },
                {
                  "type": "stringProperties",
                  "id": "{{tandem.Features.ElementAt(0).Properties.ElementAt(0).StringId}}",
                  "attributes": {
                    "name": "{{tandem.Features.ElementAt(0).Properties.ElementAt(0).Name}}"
                  },
                  "relationships": {
                    "value": {
                      "data": {
                        "type": "stringValues",
                        "id": "{{tandem.Features.ElementAt(0).Properties.OfType<StringProperty>().ElementAt(0).Value.StringId}}"
                      }
                    }
                  }
                },
                {
                  "type": "stringValues",
                  "id": "{{tandem.Features.ElementAt(0).Properties.OfType<StringProperty>().ElementAt(0).Value.StringId}}",
                  "attributes": {
                    "content": "{{tandem.Features.ElementAt(0).Properties.OfType<StringProperty>().ElementAt(0).Value.Content}}"
                  }
                },
                {
                  "type": "boxes",
                  "id": "{{tandem.CargoBox.StringId}}",
                  "attributes": {
                    "width": {{tandem.CargoBox.Width.ToString(CultureInfo.InvariantCulture)}},
                    "height": {{tandem.CargoBox.Height.ToString(CultureInfo.InvariantCulture)}},
                    "depth": {{tandem.CargoBox.Depth.ToString(CultureInfo.InvariantCulture)}}
                  }
                },
                {
                  "type": "bicycleLights",
                  "id": "{{tandem.Lights.ElementAt(0).StringId}}",
                  "attributes": {
                    "color": "{{tandem.Lights.ElementAt(0).Color}}"
                  }
                },
                {
                  "type": "vehicleManufacturers",
                  "id": "{{tandem.Manufacturer.StringId}}",
                  "attributes": {
                    "name": "{{tandem.Manufacturer.Name}}"
                  },
                  "relationships": {
                    "vehicles": {
                      "links": {
                        "self": "/vehicleManufacturers/{{tandem.Manufacturer.StringId}}/relationships/vehicles",
                        "related": "/vehicleManufacturers/{{tandem.Manufacturer.StringId}}/vehicles"
                      }
                    }
                  },
                  "links": {
                    "self": "/vehicleManufacturers/{{tandem.Manufacturer.StringId}}"
                  }
                },
                {
                  "type": "chromeWheels",
                  "id": "{{tandem.Wheels.ElementAt(0).StringId}}",
                  "attributes": {
                    "paintColor": "{{tandem.Wheels.OfType<ChromeWheel>().ElementAt(0).PaintColor}}",
                    "radius": {{tandem.Wheels.ElementAt(0).Radius.ToString(CultureInfo.InvariantCulture)}}
                  },
                  "relationships": {
                    "vehicle": {
                      "links": {
                        "self": "/chromeWheels/{{tandem.Wheels.ElementAt(0).StringId}}/relationships/vehicle",
                        "related": "/chromeWheels/{{tandem.Wheels.ElementAt(0).StringId}}/vehicle"
                      }
                    }
                  },
                  "links": {
                    "self": "/chromeWheels/{{tandem.Wheels.ElementAt(0).StringId}}"
                  }
                },
                {
                  "type": "boxes",
                  "id": "{{truck.SleepingArea.StringId}}",
                  "attributes": {
                    "width": {{truck.SleepingArea.Width.ToString(CultureInfo.InvariantCulture)}},
                    "height": {{truck.SleepingArea.Height.ToString(CultureInfo.InvariantCulture)}},
                    "depth": {{truck.SleepingArea.Depth.ToString(CultureInfo.InvariantCulture)}}
                  }
                },
                {
                  "type": "genericFeatures",
                  "id": "{{truck.Features.ElementAt(0).StringId}}",
                  "attributes": {
                    "description": "{{truck.Features.ElementAt(0).Description}}"
                  },
                  "relationships": {
                    "properties": {
                      "data": [
                        {
                          "type": "stringProperties",
                          "id": "{{truck.Features.ElementAt(0).Properties.ElementAt(0).StringId}}"
                        }
                      ]
                    }
                  }
                },
                {
                  "type": "stringProperties",
                  "id": "{{truck.Features.ElementAt(0).Properties.ElementAt(0).StringId}}",
                  "attributes": {
                    "name": "{{truck.Features.ElementAt(0).Properties.ElementAt(0).Name}}"
                  },
                  "relationships": {
                    "value": {
                      "data": {
                        "type": "stringValues",
                        "id": "{{truck.Features.ElementAt(0).Properties.OfType<StringProperty>().ElementAt(0).Value.StringId}}"
                      }
                    }
                  }
                },
                {
                  "type": "stringValues",
                  "id": "{{truck.Features.ElementAt(0).Properties.OfType<StringProperty>().ElementAt(0).Value.StringId}}",
                  "attributes": {
                    "content": "{{truck.Features.ElementAt(0).Properties.OfType<StringProperty>().ElementAt(0).Value.Content}}"
                  }
                },
                {
                  "type": "dieselEngines",
                  "id": "{{truck.Engine.StringId}}",
                  "attributes": {
                    "isHydrocarbonBased": {{truck.Engine.IsHydrocarbonBased.ToString().ToLowerInvariant()}},
                    "serialCode": "{{((DieselEngine)truck.Engine).SerialCode}}",
                    "viscosity": {{((DieselEngine)truck.Engine).Viscosity.ToString(CultureInfo.InvariantCulture)}},
                    "capacity": {{truck.Engine.Capacity.ToString(CultureInfo.InvariantCulture)}}
                  },
                  "links": {
                    "self": "/dieselEngines/{{truck.Engine.StringId}}"
                  }
                },
                {
                  "type": "navigationSystems",
                  "id": "{{truck.NavigationSystem.StringId}}",
                  "attributes": {
                    "modelType": "{{truck.NavigationSystem.ModelType}}"
                  }
                },
                {
                  "type": "vehicleManufacturers",
                  "id": "{{truck.Manufacturer.StringId}}",
                  "attributes": {
                    "name": "{{truck.Manufacturer.Name}}"
                  },
                  "relationships": {
                    "vehicles": {
                      "links": {
                        "self": "/vehicleManufacturers/{{truck.Manufacturer.StringId}}/relationships/vehicles",
                        "related": "/vehicleManufacturers/{{truck.Manufacturer.StringId}}/vehicles"
                      }
                    }
                  },
                  "links": {
                    "self": "/vehicleManufacturers/{{truck.Manufacturer.StringId}}"
                  }
                },
                {
                  "type": "chromeWheels",
                  "id": "{{truck.Wheels.ElementAt(0).StringId}}",
                  "attributes": {
                    "paintColor": "{{truck.Wheels.OfType<ChromeWheel>().ElementAt(0).PaintColor}}",
                    "radius": {{truck.Wheels.ElementAt(0).Radius.ToString(CultureInfo.InvariantCulture)}}
                  },
                  "relationships": {
                    "vehicle": {
                      "links": {
                        "self": "/chromeWheels/{{truck.Wheels.ElementAt(0).StringId}}/relationships/vehicle",
                        "related": "/chromeWheels/{{truck.Wheels.ElementAt(0).StringId}}/vehicle"
                      }
                    }
                  },
                  "links": {
                    "self": "/chromeWheels/{{truck.Wheels.ElementAt(0).StringId}}"
                  }
                }
              ]
            }
            """);
    }

    [Fact]
    public async Task Can_filter_on_derived_resource_type_at_abstract_endpoint()
    {
        // Arrange
        Bike bike = _fakers.Bike.GenerateOne();

        Tandem tandem = _fakers.Tandem.GenerateOne();

        Car car = _fakers.Car.GenerateOne();
        car.Engine = _fakers.GasolineEngine.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Vehicle>();
            dbContext.Vehicles.AddRange(bike, tandem, car);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/vehicles?filter=isType(,bikes)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "bikes" && resource.Id == bike.StringId);
        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "tandems" && resource.Id == tandem.StringId);
    }

    [Fact]
    public async Task Can_filter_on_derived_resource_type_with_condition_at_abstract_endpoint()
    {
        // Arrange
        Bike bike = _fakers.Bike.GenerateOne();

        Car car = _fakers.Car.GenerateOne();
        car.LicensePlate = "XX-99-YY";
        car.Engine = _fakers.GasolineEngine.GenerateOne();

        Truck truck = _fakers.Truck.GenerateOne();
        truck.LicensePlate = "AA-11-BB";
        truck.Engine = _fakers.DieselEngine.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Vehicle>();
            dbContext.Vehicles.AddRange(bike, car, truck);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/vehicles?filter=isType(,motorVehicles,equals(licensePlate,'AA-11-BB'))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "trucks" && resource.Id == truck.StringId);
    }

    [Fact]
    public async Task Can_filter_on_derived_resource_type_and_derived_ToOne_relationship_type_at_abstract_endpoint()
    {
        // Arrange
        Bike bike = _fakers.Bike.GenerateOne();

        Car car = _fakers.Car.GenerateOne();
        car.Engine = _fakers.GasolineEngine.GenerateOne();

        Truck truck = _fakers.Truck.GenerateOne();
        truck.Engine = _fakers.DieselEngine.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Vehicle>();
            dbContext.Vehicles.AddRange(bike, car, truck);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/vehicles?filter=isType(,motorVehicles,isType(engine,dieselEngines))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "trucks" && resource.Id == truck.StringId);
    }

    [Fact]
    public async Task Can_filter_on_derived_resource_type_and_derived_ToOne_relationship_type_with_condition_at_abstract_endpoint()
    {
        // Arrange
        Car car = _fakers.Car.GenerateOne();
        car.Engine = _fakers.GasolineEngine.GenerateOne();

        Truck truck1 = _fakers.Truck.GenerateOne();
        truck1.Engine = _fakers.DieselEngine.GenerateOne();
        ((DieselEngine)truck1.Engine).Viscosity = 25;

        Truck truck2 = _fakers.Truck.GenerateOne();
        truck2.Engine = _fakers.DieselEngine.GenerateOne();
        ((DieselEngine)truck2.Engine).Viscosity = 100;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Vehicle>();
            dbContext.Vehicles.AddRange(car, truck1, truck2);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/vehicles?filter=isType(,motorVehicles,isType(engine,dieselEngines,greaterThan(viscosity,'50')))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "trucks" && resource.Id == truck2.StringId);
    }

    [Fact]
    public async Task Can_filter_on_derived_resource_type_with_condition_at_concrete_base_endpoint()
    {
        // Arrange
        Bike bike = _fakers.Bike.GenerateOne();

        Tandem tandem1 = _fakers.Tandem.GenerateOne();

        Tandem tandem2 = _fakers.Tandem.GenerateOne();
        tandem2.Features = _fakers.GenericFeature.GenerateSet(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Vehicle>();
            dbContext.Vehicles.AddRange(bike, tandem1, tandem2);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/bikes?filter=isType(,tandems,has(features))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "tandems" && resource.Id == tandem2.StringId);
    }

    [Fact]
    public async Task Can_filter_on_derived_resource_type_with_condition_at_concrete_derived_endpoint()
    {
        // Arrange
        Car car1 = _fakers.Car.GenerateOne();
        car1.Engine = _fakers.GasolineEngine.GenerateOne();
        car1.Wheels = _fakers.CarbonWheel.GenerateSet<CarbonWheel, Wheel>(4);

        Car car2 = _fakers.Car.GenerateOne();
        car2.Engine = _fakers.GasolineEngine.GenerateOne();
        car2.Wheels = _fakers.ChromeWheel.GenerateSet<ChromeWheel, Wheel>(4);

        Car car3 = _fakers.Car.GenerateOne();
        car3.Engine = _fakers.GasolineEngine.GenerateOne();
        car3.Wheels = _fakers.ChromeWheel.GenerateSet<ChromeWheel, Wheel>(4);
        car3.Wheels.Cast<ChromeWheel>().ElementAt(0).PaintColor = "light-gray";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Vehicle>();
            dbContext.Vehicles.AddRange(car1, car2, car3);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/cars?filter=has(wheels,isType(,chromeWheels,equals(paintColor,'light-gray')))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "cars" && resource.Id == car3.StringId);
    }

    [Fact]
    public async Task Can_sort_on_derived_attribute_at_abstract_endpoint()
    {
        // Arrange
        Bike bike1 = _fakers.Bike.GenerateOne();
        bike1.GearCount = 3;

        Bike bike2 = _fakers.Bike.GenerateOne();
        bike2.GearCount = 1;

        Tandem tandem = _fakers.Tandem.GenerateOne();
        tandem.GearCount = 2;

        Car car = _fakers.Car.GenerateOne();
        car.Engine = _fakers.GasolineEngine.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Vehicle>();
            dbContext.Vehicles.AddRange(bike1, bike2, tandem, car);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/vehicles?sort=gearCount";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(4);

        responseDocument.Data.ManyValue[0].Type.Should().Be("bikes");
        responseDocument.Data.ManyValue[0].Id.Should().Be(bike2.StringId);

        responseDocument.Data.ManyValue[1].Type.Should().Be("tandems");
        responseDocument.Data.ManyValue[1].Id.Should().Be(tandem.StringId);

        responseDocument.Data.ManyValue[2].Type.Should().Be("bikes");
        responseDocument.Data.ManyValue[2].Id.Should().Be(bike1.StringId);

        responseDocument.Data.ManyValue[3].Type.Should().Be("cars");
        responseDocument.Data.ManyValue[3].Id.Should().Be(car.StringId);
    }

    [Fact]
    public async Task Can_sort_on_derived_attribute_at_concrete_base_endpoint()
    {
        // Arrange
        Bike bike = _fakers.Bike.GenerateOne();

        Tandem tandem1 = _fakers.Tandem.GenerateOne();
        tandem1.PassengerCount = 2;

        Tandem tandem2 = _fakers.Tandem.GenerateOne();
        tandem2.PassengerCount = 4;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Vehicle>();
            dbContext.Vehicles.AddRange(bike, tandem1, tandem2);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/bikes?sort=-passengerCount";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(3);

        responseDocument.Data.ManyValue[0].Type.Should().Be("bikes");
        responseDocument.Data.ManyValue[0].Id.Should().Be(bike.StringId);

        responseDocument.Data.ManyValue[1].Type.Should().Be("tandems");
        responseDocument.Data.ManyValue[1].Id.Should().Be(tandem2.StringId);

        responseDocument.Data.ManyValue[2].Type.Should().Be("tandems");
        responseDocument.Data.ManyValue[2].Id.Should().Be(tandem1.StringId);
    }

    [Fact]
    public async Task Can_sort_on_derived_relationship_at_concrete_derived_endpoint()
    {
        // Arrange
        Car car1 = _fakers.Car.GenerateOne();
        car1.Engine = _fakers.GasolineEngine.GenerateOne();
        ((GasolineEngine)car1.Engine).Cylinders = _fakers.Cylinder.GenerateSet(2);

        Car car2 = _fakers.Car.GenerateOne();
        car2.Engine = _fakers.DieselEngine.GenerateOne();

        Car car3 = _fakers.Car.GenerateOne();
        car3.Engine = _fakers.GasolineEngine.GenerateOne();
        ((GasolineEngine)car3.Engine).Cylinders = _fakers.Cylinder.GenerateSet(4);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Vehicle>();
            dbContext.Vehicles.AddRange(car1, car2, car3);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/cars?sort=-count(engine.cylinders)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(3);

        responseDocument.Data.ManyValue[0].Type.Should().Be("cars");
        responseDocument.Data.ManyValue[0].Id.Should().Be(car3.StringId);

        responseDocument.Data.ManyValue[1].Type.Should().Be("cars");
        responseDocument.Data.ManyValue[1].Id.Should().Be(car1.StringId);

        responseDocument.Data.ManyValue[2].Type.Should().Be("cars");
        responseDocument.Data.ManyValue[2].Id.Should().Be(car2.StringId);
    }

    [Fact]
    public async Task Cannot_sort_on_ambiguous_derived_attribute()
    {
        // Arrange
        var parameterValue = new MarkedText("engine.^serialCode", '^');
        string route = $"/cars?sort={parameterValue.Text}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified sort is invalid.");
        error.Detail.Should().Be($"Field 'serialCode' is defined on multiple types that derive from resource type 'engines'. {parameterValue}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("sort");
    }

    [Fact]
    public async Task Cannot_sort_on_ambiguous_derived_relationship()
    {
        // Arrange
        var parameterValue = new MarkedText("count(^features)", '^');
        string route = $"/vehicles?sort={parameterValue.Text}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified sort is invalid.");
        error.Detail.Should().Be($"Field 'features' is defined on multiple types that derive from resource type 'vehicles'. {parameterValue}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("sort");
    }

    [Fact]
    public async Task Can_sort_on_derived_attribute_from_resource_definition_using_expression_syntax()
    {
        // Arrange
        ChromeWheel chromeWheel1 = _fakers.ChromeWheel.GenerateOne();
        chromeWheel1.PaintColor = "blue";
        chromeWheel1.Vehicle = _fakers.Car.GenerateOne();
        ((Car)chromeWheel1.Vehicle).Engine = _fakers.GasolineEngine.GenerateOne();
        ((GasolineEngine)((Car)chromeWheel1.Vehicle).Engine).Cylinders = _fakers.Cylinder.GenerateSet(2);

        ChromeWheel chromeWheel2 = _fakers.ChromeWheel.GenerateOne();
        chromeWheel2.PaintColor = "blue";
        chromeWheel2.Vehicle = _fakers.Car.GenerateOne();
        ((Car)chromeWheel2.Vehicle).Engine = _fakers.GasolineEngine.GenerateOne();
        ((GasolineEngine)((Car)chromeWheel2.Vehicle).Engine).Cylinders = _fakers.Cylinder.GenerateSet(1);

        ChromeWheel chromeWheel3 = _fakers.ChromeWheel.GenerateOne();
        chromeWheel3.PaintColor = "black";

        CarbonWheel carbonWheel1 = _fakers.CarbonWheel.GenerateOne();
        carbonWheel1.HasTube = false;

        CarbonWheel carbonWheel2 = _fakers.CarbonWheel.GenerateOne();
        carbonWheel2.HasTube = true;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Wheel>();
            dbContext.Wheels.AddRange(chromeWheel1, chromeWheel2, chromeWheel3, carbonWheel1, carbonWheel2);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/wheels?autoSort=expr";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(5);

        responseDocument.Data.ManyValue[0].Type.Should().Be("chromeWheels");
        responseDocument.Data.ManyValue[0].Id.Should().Be(chromeWheel3.StringId);

        responseDocument.Data.ManyValue[1].Type.Should().Be("chromeWheels");
        responseDocument.Data.ManyValue[1].Id.Should().Be(chromeWheel2.StringId);

        responseDocument.Data.ManyValue[2].Type.Should().Be("chromeWheels");
        responseDocument.Data.ManyValue[2].Id.Should().Be(chromeWheel1.StringId);

        responseDocument.Data.ManyValue[3].Type.Should().Be("carbonWheels");
        responseDocument.Data.ManyValue[3].Id.Should().Be(carbonWheel2.StringId);

        responseDocument.Data.ManyValue[4].Type.Should().Be("carbonWheels");
        responseDocument.Data.ManyValue[4].Id.Should().Be(carbonWheel1.StringId);
    }

    [Fact]
    public async Task Can_sort_on_derived_attribute_from_resource_definition_using_lambda_syntax()
    {
        // Arrange
        ChromeWheel chromeWheel1 = _fakers.ChromeWheel.GenerateOne();
        chromeWheel1.PaintColor = "blue";
        chromeWheel1.Vehicle = _fakers.Car.GenerateOne();
        ((Car)chromeWheel1.Vehicle).Engine = _fakers.GasolineEngine.GenerateOne();
        ((GasolineEngine)((Car)chromeWheel1.Vehicle).Engine).Cylinders = _fakers.Cylinder.GenerateSet(2);

        ChromeWheel chromeWheel2 = _fakers.ChromeWheel.GenerateOne();
        chromeWheel2.PaintColor = "blue";
        chromeWheel2.Vehicle = _fakers.Car.GenerateOne();
        ((Car)chromeWheel2.Vehicle).Engine = _fakers.GasolineEngine.GenerateOne();
        ((GasolineEngine)((Car)chromeWheel2.Vehicle).Engine).Cylinders = _fakers.Cylinder.GenerateSet(1);

        ChromeWheel chromeWheel3 = _fakers.ChromeWheel.GenerateOne();
        chromeWheel3.PaintColor = "black";

        CarbonWheel carbonWheel1 = _fakers.CarbonWheel.GenerateOne();
        carbonWheel1.HasTube = false;

        CarbonWheel carbonWheel2 = _fakers.CarbonWheel.GenerateOne();
        carbonWheel2.HasTube = true;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Wheel>();
            dbContext.Wheels.AddRange(chromeWheel1, chromeWheel2, chromeWheel3, carbonWheel1, carbonWheel2);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/wheels?autoSort=lambda";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(5);

        responseDocument.Data.ManyValue[0].Type.Should().Be("chromeWheels");
        responseDocument.Data.ManyValue[0].Id.Should().Be(chromeWheel3.StringId);

        responseDocument.Data.ManyValue[1].Type.Should().Be("chromeWheels");
        responseDocument.Data.ManyValue[1].Id.Should().Be(chromeWheel2.StringId);

        responseDocument.Data.ManyValue[2].Type.Should().Be("chromeWheels");
        responseDocument.Data.ManyValue[2].Id.Should().Be(chromeWheel1.StringId);

        responseDocument.Data.ManyValue[3].Type.Should().Be("carbonWheels");
        responseDocument.Data.ManyValue[3].Id.Should().Be(carbonWheel2.StringId);

        responseDocument.Data.ManyValue[4].Type.Should().Be("carbonWheels");
        responseDocument.Data.ManyValue[4].Id.Should().Be(carbonWheel1.StringId);
    }
}
