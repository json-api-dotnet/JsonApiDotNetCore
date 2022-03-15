using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Models;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance;

public abstract class ResourceInheritanceTests<TDbContext> : IClassFixture<IntegrationTestContext<TestableStartup<TDbContext>, TDbContext>>
    where TDbContext : ResourceInheritanceDbContext
{
    private readonly IntegrationTestContext<TestableStartup<TDbContext>, TDbContext> _testContext;
    private readonly ResourceInheritanceFakers _fakers = new();

    protected ResourceInheritanceTests(IntegrationTestContext<TestableStartup<TDbContext>, TDbContext> testContext)
    {
        _testContext = testContext;

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

        var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.UseRelativeLinks = true;
    }

    [Fact]
    public async Task Can_get_primary_resources_at_abstract_endpoint()
    {
        // Arrange
        Bike bike = _fakers.Bike.Generate();

        Tandem tandem = _fakers.Tandem.Generate();

        Car car = _fakers.Car.Generate();
        car.Engine = _fakers.GasolineEngine.Generate();

        Truck truck = _fakers.Truck.Generate();
        truck.Engine = _fakers.DieselEngine.Generate();

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
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Links.ShouldNotBeNull();
        responseDocument.Links.Self.Should().Be(route);

        responseDocument.Data.ManyValue.ShouldHaveCount(4);

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "bikes").Subject.With(resource =>
        {
            resource.Id.Should().Be(bike.StringId);

            resource.Links.ShouldNotBeNull();
            resource.Links.Self.Should().Be($"/bikes/{bike.StringId}");

            resource.Attributes.ShouldHaveCount(3);
            resource.Attributes.ShouldContainKey("weight").With(value => value.Should().Be(bike.Weight));
            resource.Attributes.ShouldContainKey("requiresDriverLicense").With(value => value.Should().Be(bike.RequiresDriverLicense));
            resource.Attributes.ShouldContainKey("gearCount").With(value => value.Should().Be(bike.GearCount));

            resource.Relationships.ShouldOnlyContainKeys("manufacturer", "wheels", "cargoBox", "lights");

            foreach ((string? name, RelationshipObject? value) in resource.Relationships)
            {
                value.ShouldNotBeNull();
                value.Links.ShouldNotBeNull();
                value.Links.Self.Should().Be($"/bikes/{bike.StringId}/relationships/{name}");
                value.Links.Related.Should().Be($"/bikes/{bike.StringId}/{name}");
            }
        });

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "tandems").Subject.With(resource =>
        {
            resource.Id.Should().Be(tandem.StringId);

            resource.Links.ShouldNotBeNull();
            resource.Links.Self.Should().Be($"/tandems/{tandem.StringId}");

            resource.Attributes.ShouldHaveCount(4);
            resource.Attributes.ShouldContainKey("weight").With(value => value.Should().Be(tandem.Weight));
            resource.Attributes.ShouldContainKey("requiresDriverLicense").With(value => value.Should().Be(tandem.RequiresDriverLicense));
            resource.Attributes.ShouldContainKey("gearCount").With(value => value.Should().Be(tandem.GearCount));
            resource.Attributes.ShouldContainKey("passengerCount").With(value => value.Should().Be(tandem.PassengerCount));

            resource.Relationships.ShouldOnlyContainKeys("manufacturer", "wheels", "cargoBox", "lights", "features");

            foreach ((string? name, RelationshipObject? value) in resource.Relationships)
            {
                value.ShouldNotBeNull();
                value.Links.ShouldNotBeNull();
                value.Links.Self.Should().Be($"/tandems/{tandem.StringId}/relationships/{name}");
                value.Links.Related.Should().Be($"/tandems/{tandem.StringId}/{name}");
            }
        });

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "cars").Subject.With(resource =>
        {
            resource.Id.Should().Be(car.StringId);

            resource.Links.ShouldNotBeNull();
            resource.Links.Self.Should().Be($"/cars/{car.StringId}");

            resource.Attributes.ShouldHaveCount(4);
            resource.Attributes.ShouldContainKey("weight").With(value => value.Should().Be(car.Weight));
            resource.Attributes.ShouldContainKey("requiresDriverLicense").With(value => value.Should().Be(car.RequiresDriverLicense));
            resource.Attributes.ShouldContainKey("licensePlate").With(value => value.Should().Be(car.LicensePlate));
            resource.Attributes.ShouldContainKey("seatCount").With(value => value.Should().Be(car.SeatCount));

            resource.Relationships.ShouldOnlyContainKeys("manufacturer", "wheels", "engine", "navigationSystem", "features");

            foreach ((string? name, RelationshipObject? value) in resource.Relationships)
            {
                value.ShouldNotBeNull();
                value.Links.ShouldNotBeNull();
                value.Links.Self.Should().Be($"/cars/{car.StringId}/relationships/{name}");
                value.Links.Related.Should().Be($"/cars/{car.StringId}/{name}");
            }
        });

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "trucks").Subject.With(resource =>
        {
            resource.Id.Should().Be(truck.StringId);

            resource.Links.ShouldNotBeNull();
            resource.Links.Self.Should().Be($"/trucks/{truck.StringId}");

            resource.Attributes.ShouldHaveCount(4);
            resource.Attributes.ShouldContainKey("weight").With(value => value.Should().Be(truck.Weight));
            resource.Attributes.ShouldContainKey("requiresDriverLicense").With(value => value.Should().Be(truck.RequiresDriverLicense));
            resource.Attributes.ShouldContainKey("licensePlate").With(value => value.Should().Be(truck.LicensePlate));
            resource.Attributes.ShouldContainKey("loadingCapacity").With(value => value.Should().Be(truck.LoadingCapacity));

            resource.Relationships.ShouldOnlyContainKeys("manufacturer", "wheels", "engine", "navigationSystem", "sleepingArea", "features");

            foreach ((string? name, RelationshipObject? value) in resource.Relationships)
            {
                value.ShouldNotBeNull();
                value.Links.ShouldNotBeNull();
                value.Links.Self.Should().Be($"/trucks/{truck.StringId}/relationships/{name}");
                value.Links.Related.Should().Be($"/trucks/{truck.StringId}/{name}");
            }
        });
    }

    [Fact]
    public async Task Can_get_primary_resources_at_concrete_base_endpoint()
    {
        // Arrange
        Bike bike = _fakers.Bike.Generate();
        Tandem tandem = _fakers.Tandem.Generate();

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
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Links.ShouldNotBeNull();
        responseDocument.Links.Self.Should().Be(route);

        responseDocument.Data.ManyValue.ShouldHaveCount(2);

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "bikes").Subject.With(resource =>
        {
            resource.Id.Should().Be(bike.StringId);

            resource.Links.ShouldNotBeNull();
            resource.Links.Self.Should().Be($"/bikes/{bike.StringId}");

            resource.Attributes.ShouldHaveCount(3);
            resource.Attributes.ShouldContainKey("weight").With(value => value.Should().Be(bike.Weight));
            resource.Attributes.ShouldContainKey("requiresDriverLicense").With(value => value.Should().Be(bike.RequiresDriverLicense));
            resource.Attributes.ShouldContainKey("gearCount").With(value => value.Should().Be(bike.GearCount));

            resource.Relationships.ShouldOnlyContainKeys("manufacturer", "wheels", "cargoBox", "lights");

            foreach ((string? name, RelationshipObject? value) in resource.Relationships)
            {
                value.ShouldNotBeNull();
                value.Links.ShouldNotBeNull();
                value.Links.Self.Should().Be($"/bikes/{bike.StringId}/relationships/{name}");
                value.Links.Related.Should().Be($"/bikes/{bike.StringId}/{name}");
            }
        });

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "tandems").Subject.With(resource =>
        {
            resource.Id.Should().Be(tandem.StringId);

            resource.Links.ShouldNotBeNull();
            resource.Links.Self.Should().Be($"/tandems/{tandem.StringId}");

            resource.Attributes.ShouldHaveCount(4);
            resource.Attributes.ShouldContainKey("weight").With(value => value.Should().Be(tandem.Weight));
            resource.Attributes.ShouldContainKey("requiresDriverLicense").With(value => value.Should().Be(tandem.RequiresDriverLicense));
            resource.Attributes.ShouldContainKey("gearCount").With(value => value.Should().Be(tandem.GearCount));
            resource.Attributes.ShouldContainKey("passengerCount").With(value => value.Should().Be(tandem.PassengerCount));

            resource.Relationships.ShouldOnlyContainKeys("manufacturer", "wheels", "cargoBox", "lights", "features");

            foreach ((string? name, RelationshipObject? value) in resource.Relationships)
            {
                value.ShouldNotBeNull();
                value.Links.ShouldNotBeNull();
                value.Links.Self.Should().Be($"/tandems/{tandem.StringId}/relationships/{name}");
                value.Links.Related.Should().Be($"/tandems/{tandem.StringId}/{name}");
            }
        });
    }

    [Fact]
    public async Task Can_get_primary_resources_at_concrete_derived_endpoint()
    {
        // Arrange
        Bike bike = _fakers.Bike.Generate();
        Tandem tandem = _fakers.Tandem.Generate();

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
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Links.ShouldNotBeNull();
        responseDocument.Links.Self.Should().Be(route);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "tandems").Subject.With(resource =>
        {
            resource.Id.Should().Be(tandem.StringId);

            resource.Links.ShouldNotBeNull();
            resource.Links.Self.Should().Be($"/tandems/{tandem.StringId}");

            resource.Attributes.ShouldHaveCount(4);
            resource.Attributes.ShouldContainKey("weight").With(value => value.Should().Be(tandem.Weight));
            resource.Attributes.ShouldContainKey("requiresDriverLicense").With(value => value.Should().Be(tandem.RequiresDriverLicense));
            resource.Attributes.ShouldContainKey("gearCount").With(value => value.Should().Be(tandem.GearCount));
            resource.Attributes.ShouldContainKey("passengerCount").With(value => value.Should().Be(tandem.PassengerCount));

            resource.Relationships.ShouldOnlyContainKeys("manufacturer", "wheels", "cargoBox", "lights", "features");

            foreach ((string? name, RelationshipObject? value) in resource.Relationships)
            {
                value.ShouldNotBeNull();
                value.Links.ShouldNotBeNull();
                value.Links.Self.Should().Be($"/tandems/{tandem.StringId}/relationships/{name}");
                value.Links.Related.Should().Be($"/tandems/{tandem.StringId}/{name}");
            }
        });
    }

    [Fact]
    public async Task Can_get_primary_resource_at_abstract_endpoint()
    {
        // Arrange
        Tandem tandem = _fakers.Tandem.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(tandem);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/vehicles/{tandem.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("tandems");
        responseDocument.Data.SingleValue.Id.Should().Be(tandem.StringId);
        responseDocument.Data.SingleValue.Attributes.ShouldOnlyContainKeys("weight", "requiresDriverLicense", "gearCount", "passengerCount");
        responseDocument.Data.SingleValue.Relationships.ShouldOnlyContainKeys("manufacturer", "wheels", "cargoBox", "lights", "features");
    }

    [Fact]
    public async Task Can_get_primary_resource_at_concrete_base_endpoint()
    {
        // Arrange
        Tandem tandem = _fakers.Tandem.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(tandem);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/bikes/{tandem.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("tandems");
        responseDocument.Data.SingleValue.Id.Should().Be(tandem.StringId);
        responseDocument.Data.SingleValue.Attributes.ShouldOnlyContainKeys("weight", "requiresDriverLicense", "gearCount", "passengerCount");
        responseDocument.Data.SingleValue.Relationships.ShouldOnlyContainKeys("manufacturer", "wheels", "cargoBox", "lights", "features");
    }

    [Fact]
    public async Task Can_get_primary_resource_at_concrete_derived_endpoint()
    {
        // Arrange
        Tandem tandem = _fakers.Tandem.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(tandem);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/tandems/{tandem.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("tandems");
        responseDocument.Data.SingleValue.Id.Should().Be(tandem.StringId);
        responseDocument.Data.SingleValue.Attributes.ShouldOnlyContainKeys("weight", "requiresDriverLicense", "gearCount", "passengerCount");
        responseDocument.Data.SingleValue.Relationships.ShouldOnlyContainKeys("manufacturer", "wheels", "cargoBox", "lights", "features");
    }

    [Fact]
    public async Task Cannot_get_secondary_resource_defined_in_derived_type_at_abstract_endpoint()
    {
        // Arrange
        Car car = _fakers.Car.Generate();
        car.Engine = _fakers.GasolineEngine.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(car);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/vehicles/{car.StringId}/engine";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested relationship does not exist.");
        error.Detail.Should().Be("Resource of type 'vehicles' does not contain a relationship named 'engine'.");
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Can_get_secondary_resource_at_concrete_derived_endpoint()
    {
        // Arrange
        Car car = _fakers.Car.Generate();
        car.Engine = _fakers.GasolineEngine.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(car);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/cars/{car.StringId}/engine";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Links.ShouldNotBeNull();
        responseDocument.Links.Self.Should().Be(route);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("gasolineEngines");
        responseDocument.Data.SingleValue.Id.Should().Be(car.Engine.StringId);

        responseDocument.Data.SingleValue.Links.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Links.Self.Should().Be($"/gasolineEngines/{car.Engine.StringId}");

        responseDocument.Data.SingleValue.Attributes.ShouldHaveCount(4);
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("isHydrocarbonBased").With(value => value.Should().Be(car.Engine.IsHydrocarbonBased));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("capacity").With(value => value.Should().Be(car.Engine.Capacity));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("serialCode").With(value => value.Should().Be(((GasolineEngine)car.Engine).SerialCode));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("volatility").With(value => value.Should().Be(((GasolineEngine)car.Engine).Volatility));

        responseDocument.Data.SingleValue.Relationships.ShouldHaveCount(1);

        responseDocument.Data.SingleValue.Relationships.ShouldContainKey("cylinders").With(value =>
        {
            value.ShouldNotBeNull();
            value.Links.ShouldNotBeNull();
            value.Links.Self.Should().Be($"/gasolineEngines/{car.Engine.StringId}/relationships/cylinders");
            value.Links.Related.Should().Be($"/gasolineEngines/{car.Engine.StringId}/cylinders");
        });
    }

    [Fact]
    public async Task Cannot_get_secondary_resources_defined_in_derived_type_at_abstract_endpoint()
    {
        // Arrange
        Truck truck = _fakers.Truck.Generate();
        truck.Engine = _fakers.DieselEngine.Generate();
        truck.Features = _fakers.GenericFeature.Generate(1).ToHashSet();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(truck);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/vehicles/{truck.StringId}/features";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(1);

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
        Tandem tandem = _fakers.Tandem.Generate();
        tandem.Features = _fakers.GenericFeature.Generate(1).ToHashSet();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(tandem);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/bikes/{tandem.StringId}/features";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested relationship does not exist.");
        error.Detail.Should().Be("Resource of type 'bikes' does not contain a relationship named 'features'.");
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Can_get_secondary_resources_at_concrete_derived_endpoint()
    {
        // Arrange
        Car car = _fakers.Car.Generate();
        car.Engine = _fakers.GasolineEngine.Generate();
        car.Wheels = _fakers.ChromeWheel.Generate(2).Cast<Wheel>().Concat(_fakers.CarbonWheel.Generate(2)).ToHashSet();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(car);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/cars/{car.StringId}/wheels";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Links.ShouldNotBeNull();
        responseDocument.Links.Self.Should().Be(route);

        responseDocument.Data.ManyValue.ShouldHaveCount(4);

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "chromeWheels" && resource.Id == car.Wheels.ElementAt(0).StringId);
        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "chromeWheels" && resource.Id == car.Wheels.ElementAt(1).StringId);
        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "carbonWheels" && resource.Id == car.Wheels.ElementAt(2).StringId);
        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "carbonWheels" && resource.Id == car.Wheels.ElementAt(3).StringId);

        foreach (ResourceObject resource in responseDocument.Data.ManyValue.Where(value => value.Type == "chromeWheels"))
        {
            resource.Links.ShouldNotBeNull();
            resource.Links.Self.Should().Be($"/chromeWheels/{resource.Id}");

            resource.Attributes.ShouldHaveCount(2);
            resource.Attributes.ShouldContainKey("radius");
            resource.Attributes.ShouldContainKey("paintColor");
        }

        foreach (ResourceObject resource in responseDocument.Data.ManyValue.Where(value => value.Type == "carbonWheels"))
        {
            resource.Links.ShouldNotBeNull();
            resource.Links.Self.Should().Be($"/carbonWheels/{resource.Id}");

            resource.Attributes.ShouldHaveCount(2);
            resource.Attributes.ShouldContainKey("radius");
            resource.Attributes.ShouldContainKey("hasTube");
        }

        foreach (ResourceObject resource in responseDocument.Data.ManyValue)
        {
            resource.Relationships.ShouldHaveCount(1);

            resource.Relationships.ShouldContainKey("vehicle").With(value =>
            {
                value.ShouldNotBeNull();
                value.Links.ShouldNotBeNull();
                value.Links.Self.Should().Be($"/{resource.Type}/{resource.Id}/relationships/vehicle");
                value.Links.Related.Should().Be($"/{resource.Type}/{resource.Id}/vehicle");
            });
        }
    }
}
