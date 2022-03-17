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

    [Fact]
    public async Task Cannot_get_ToOne_relationship_defined_in_derived_type_at_abstract_endpoint()
    {
        // Arrange
        Car car = _fakers.Car.Generate();
        car.Engine = _fakers.GasolineEngine.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(car);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/vehicles/{car.StringId}/relationships/engine";

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
    public async Task Can_get_ToOne_relationship_at_concrete_derived_endpoint()
    {
        // Arrange
        Car car = _fakers.Car.Generate();
        car.Engine = _fakers.GasolineEngine.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(car);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/cars/{car.StringId}/relationships/engine";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Links.ShouldNotBeNull();
        responseDocument.Links.Self.Should().Be(route);
        responseDocument.Links.Related.Should().Be($"/cars/{car.StringId}/engine");

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("gasolineEngines");
        responseDocument.Data.SingleValue.Id.Should().Be(car.Engine.StringId);
        responseDocument.Data.SingleValue.Links.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_get_ToMany_relationship_defined_in_derived_type_at_abstract_endpoint()
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

        string route = $"/vehicles/{truck.StringId}/relationships/features";

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
    public async Task Cannot_get_ToMany_relationship_defined_in_derived_type_at_concrete_base_endpoint()
    {
        // Arrange
        Tandem tandem = _fakers.Tandem.Generate();
        tandem.Features = _fakers.GenericFeature.Generate(1).ToHashSet();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(tandem);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/bikes/{tandem.StringId}/relationships/features";

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
    public async Task Can_get_ToMany_relationship_at_concrete_derived_endpoint()
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

        string route = $"/cars/{car.StringId}/relationships/wheels";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Links.ShouldNotBeNull();
        responseDocument.Links.Self.Should().Be(route);
        responseDocument.Links.Related.Should().Be($"/cars/{car.StringId}/wheels");

        responseDocument.Data.ManyValue.ShouldHaveCount(4);

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
    public async Task Can_get_primary_resources_at_abstract_endpoint_with_all_sparse_fieldsets()
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

        const string bikeFields = "fields[bikes]=weight,gearCount,lights";
        const string tandemFields = "fields[tandems]=gearCount,passengerCount,cargoBox";
        const string carFields = "fields[cars]=weight,requiresDriverLicense,seatCount,engine";
        const string truckFields = "fields[trucks]=loadingCapacity,sleepingArea";

        const string route = $"/vehicles?{bikeFields}&{tandemFields}&{carFields}&{truckFields}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(4);

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "bikes").Subject.With(resource =>
        {
            resource.Id.Should().Be(bike.StringId);
            resource.Attributes.ShouldOnlyContainKeys("weight", "gearCount");
            resource.Relationships.ShouldOnlyContainKeys("lights");
        });

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "tandems").Subject.With(resource =>
        {
            resource.Id.Should().Be(tandem.StringId);
            resource.Attributes.ShouldOnlyContainKeys("gearCount", "passengerCount");
            resource.Relationships.ShouldOnlyContainKeys("cargoBox");
        });

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "cars").Subject.With(resource =>
        {
            resource.Id.Should().Be(car.StringId);
            resource.Attributes.ShouldOnlyContainKeys("weight", "requiresDriverLicense", "seatCount");
            resource.Relationships.ShouldOnlyContainKeys("engine");
        });

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "trucks").Subject.With(resource =>
        {
            resource.Id.Should().Be(truck.StringId);
            resource.Attributes.ShouldOnlyContainKeys("loadingCapacity");
            resource.Relationships.ShouldOnlyContainKeys("sleepingArea");
        });
    }

    [Fact]
    public async Task Can_get_primary_resources_at_abstract_endpoint_with_some_sparse_fieldsets()
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

        const string bikeFields = "fields[bikes]=weight,gearCount,lights";
        const string carFields = "fields[cars]=weight,requiresDriverLicense,seatCount,engine";

        const string route = $"/vehicles?{bikeFields}&{carFields}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(4);

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "bikes").Subject.With(resource =>
        {
            resource.Id.Should().Be(bike.StringId);
            resource.Attributes.ShouldOnlyContainKeys("weight", "gearCount");
            resource.Relationships.ShouldOnlyContainKeys("lights");
        });

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "tandems").Subject.With(resource =>
        {
            resource.Id.Should().Be(tandem.StringId);
            resource.Attributes.ShouldOnlyContainKeys("weight", "requiresDriverLicense", "gearCount", "passengerCount");
            resource.Relationships.ShouldOnlyContainKeys("manufacturer", "wheels", "lights", "cargoBox", "features");
        });

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "cars").Subject.With(resource =>
        {
            resource.Id.Should().Be(car.StringId);
            resource.Attributes.ShouldOnlyContainKeys("weight", "requiresDriverLicense", "seatCount");
            resource.Relationships.ShouldOnlyContainKeys("engine");
        });

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "trucks").Subject.With(resource =>
        {
            resource.Id.Should().Be(truck.StringId);
            resource.Attributes.ShouldOnlyContainKeys("weight", "requiresDriverLicense", "licensePlate", "loadingCapacity");
            resource.Relationships.ShouldOnlyContainKeys("manufacturer", "wheels", "engine", "navigationSystem", "sleepingArea", "features");
        });
    }

    [Fact]
    public async Task Can_get_primary_resources_at_concrete_base_endpoint_with_all_sparse_fieldsets()
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

        const string bikeFields = "fields[bikes]=weight,gearCount,manufacturer,lights";
        const string tandemFields = "fields[tandems]=passengerCount,cargoBox";

        const string route = $"/bikes?{bikeFields}&{tandemFields}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(2);

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "bikes").Subject.With(resource =>
        {
            resource.Id.Should().Be(bike.StringId);

            resource.Attributes.ShouldOnlyContainKeys("weight", "gearCount");
            resource.Relationships.ShouldOnlyContainKeys("manufacturer", "lights");
        });

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "tandems").Subject.With(resource =>
        {
            resource.Id.Should().Be(tandem.StringId);

            resource.Attributes.ShouldOnlyContainKeys("passengerCount");
            resource.Relationships.ShouldOnlyContainKeys("cargoBox");
        });
    }

    [Fact]
    public async Task Can_get_primary_resources_at_concrete_base_endpoint_with_some_sparse_fieldsets()
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

        const string tandemFields = "fields[tandems]=passengerCount,cargoBox";

        const string route = $"/bikes?{tandemFields}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(2);

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "bikes").Subject.With(resource =>
        {
            resource.Id.Should().Be(bike.StringId);

            resource.Attributes.ShouldOnlyContainKeys("weight", "requiresDriverLicense", "gearCount");
            resource.Relationships.ShouldOnlyContainKeys("manufacturer", "wheels", "cargoBox", "lights");
        });

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "tandems").Subject.With(resource =>
        {
            resource.Id.Should().Be(tandem.StringId);

            resource.Attributes.ShouldOnlyContainKeys("passengerCount");
            resource.Relationships.ShouldOnlyContainKeys("cargoBox");
        });
    }

    [Fact]
    public async Task Can_get_primary_resources_at_abstract_endpoint_with_derived_includes()
    {
        // Arrange
        Bike bike = _fakers.Bike.Generate();
        bike.Manufacturer = _fakers.VehicleManufacturer.Generate();
        bike.Wheels = _fakers.CarbonWheel.Generate(1).Cast<Wheel>().ToHashSet();
        bike.CargoBox = _fakers.Box.Generate();
        bike.Lights = _fakers.BicycleLight.Generate(1).ToHashSet();

        Tandem tandem = _fakers.Tandem.Generate();
        tandem.Manufacturer = _fakers.VehicleManufacturer.Generate();
        tandem.Wheels = _fakers.ChromeWheel.Generate(1).Cast<Wheel>().ToHashSet();
        tandem.CargoBox = _fakers.Box.Generate();
        tandem.Lights = _fakers.BicycleLight.Generate(1).ToHashSet();
        tandem.Features = _fakers.GenericFeature.Generate(1).ToHashSet();
        tandem.Features.ElementAt(0).Properties = _fakers.StringProperty.Generate(1).Cast<GenericProperty>().ToHashSet();
        ((StringProperty)tandem.Features.ElementAt(0).Properties.ElementAt(0)).Value = _fakers.StringValue.Generate();

        Car car = _fakers.Car.Generate();
        car.Manufacturer = _fakers.VehicleManufacturer.Generate();
        car.Wheels = _fakers.CarbonWheel.Generate(1).Cast<Wheel>().ToHashSet();
        car.Engine = _fakers.GasolineEngine.Generate();
        ((GasolineEngine)car.Engine).Cylinders = _fakers.Cylinder.Generate(1).ToHashSet();
        car.NavigationSystem = _fakers.NavigationSystem.Generate();
        car.Features = _fakers.GenericFeature.Generate(1).ToHashSet();
        car.Features.ElementAt(0).Properties = _fakers.NumberProperty.Generate(1).Cast<GenericProperty>().ToHashSet();
        ((NumberProperty)car.Features.ElementAt(0).Properties.ElementAt(0)).Value = _fakers.NumberValue.Generate();

        Truck truck = _fakers.Truck.Generate();
        truck.Manufacturer = _fakers.VehicleManufacturer.Generate();
        truck.Wheels = _fakers.ChromeWheel.Generate(1).Cast<Wheel>().ToHashSet();
        truck.Engine = _fakers.DieselEngine.Generate();
        truck.NavigationSystem = _fakers.NavigationSystem.Generate();
        truck.SleepingArea = _fakers.Box.Generate();
        truck.Features = _fakers.GenericFeature.Generate(1).ToHashSet();
        truck.Features.ElementAt(0).Properties = _fakers.StringProperty.Generate(1).Cast<GenericProperty>().ToHashSet();
        ((StringProperty)truck.Features.ElementAt(0).Properties.ElementAt(0)).Value = _fakers.StringValue.Generate();

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
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Should().BeJson($@"{{
  ""links"": {{
    ""self"": ""{route}"",
    ""first"": ""{route}""
  }},
  ""data"": [
    {{
      ""type"": ""bikes"",
      ""id"": ""{bike.StringId}"",
      ""attributes"": {{
        ""requiresDriverLicense"": {bike.RequiresDriverLicense.ToString().ToLowerInvariant()},
        ""gearCount"": {bike.GearCount},
        ""weight"": {bike.Weight.ToString(CultureInfo.InvariantCulture)}
      }},
      ""relationships"": {{
        ""cargoBox"": {{
          ""links"": {{
            ""self"": ""/bikes/{bike.StringId}/relationships/cargoBox"",
            ""related"": ""/bikes/{bike.StringId}/cargoBox""
          }},
          ""data"": {{
            ""type"": ""boxes"",
            ""id"": ""{bike.CargoBox.StringId}""
          }}
        }},
        ""lights"": {{
          ""links"": {{
            ""self"": ""/bikes/{bike.StringId}/relationships/lights"",
            ""related"": ""/bikes/{bike.StringId}/lights""
          }},
          ""data"": [
            {{
              ""type"": ""bicycleLights"",
              ""id"": ""{bike.Lights.ElementAt(0).StringId}""
            }}
          ]
        }},
        ""manufacturer"": {{
          ""links"": {{
            ""self"": ""/bikes/{bike.StringId}/relationships/manufacturer"",
            ""related"": ""/bikes/{bike.StringId}/manufacturer""
          }},
          ""data"": {{
            ""type"": ""vehicleManufacturers"",
            ""id"": ""{bike.Manufacturer.StringId}""
          }}
        }},
        ""wheels"": {{
          ""links"": {{
            ""self"": ""/bikes/{bike.StringId}/relationships/wheels"",
            ""related"": ""/bikes/{bike.StringId}/wheels""
          }},
          ""data"": [
            {{
              ""type"": ""carbonWheels"",
              ""id"": ""{bike.Wheels.OfType<CarbonWheel>().ElementAt(0).StringId}""
            }}
          ]
        }}
      }},
      ""links"": {{
        ""self"": ""/bikes/{bike.StringId}""
      }}
    }},
    {{
      ""type"": ""cars"",
      ""id"": ""{car.StringId}"",
      ""attributes"": {{
        ""seatCount"": {car.SeatCount},
        ""requiresDriverLicense"": {car.RequiresDriverLicense.ToString().ToLowerInvariant()},
        ""licensePlate"": ""{car.LicensePlate}"",
        ""weight"": {car.Weight.ToString(CultureInfo.InvariantCulture)}
      }},
      ""relationships"": {{
        ""features"": {{
          ""links"": {{
            ""self"": ""/cars/{car.StringId}/relationships/features"",
            ""related"": ""/cars/{car.StringId}/features""
          }},
          ""data"": [
            {{
              ""type"": ""genericFeatures"",
              ""id"": ""{car.Features.ElementAt(0).StringId}""
            }}
          ]
        }},
        ""engine"": {{
          ""links"": {{
            ""self"": ""/cars/{car.StringId}/relationships/engine"",
            ""related"": ""/cars/{car.StringId}/engine""
          }},
          ""data"": {{
            ""type"": ""gasolineEngines"",
            ""id"": ""{car.Engine.StringId}""
          }}
        }},
        ""navigationSystem"": {{
          ""links"": {{
            ""self"": ""/cars/{car.StringId}/relationships/navigationSystem"",
            ""related"": ""/cars/{car.StringId}/navigationSystem""
          }},
          ""data"": {{
            ""type"": ""navigationSystems"",
            ""id"": ""{car.NavigationSystem.StringId}""
          }}
        }},
        ""manufacturer"": {{
          ""links"": {{
            ""self"": ""/cars/{car.StringId}/relationships/manufacturer"",
            ""related"": ""/cars/{car.StringId}/manufacturer""
          }},
          ""data"": {{
            ""type"": ""vehicleManufacturers"",
            ""id"": ""{car.Manufacturer.StringId}""
          }}
        }},
        ""wheels"": {{
          ""links"": {{
            ""self"": ""/cars/{car.StringId}/relationships/wheels"",
            ""related"": ""/cars/{car.StringId}/wheels""
          }},
          ""data"": [
            {{
              ""type"": ""carbonWheels"",
              ""id"": ""{car.Wheels.OfType<CarbonWheel>().ElementAt(0).StringId}""
            }}
          ]
        }}
      }},
      ""links"": {{
        ""self"": ""/cars/{car.StringId}""
      }}
    }},
    {{
      ""type"": ""tandems"",
      ""id"": ""{tandem.StringId}"",
      ""attributes"": {{
        ""passengerCount"": {tandem.PassengerCount},
        ""requiresDriverLicense"": {tandem.RequiresDriverLicense.ToString().ToLowerInvariant()},
        ""gearCount"": {tandem.GearCount},
        ""weight"": {tandem.Weight.ToString(CultureInfo.InvariantCulture)}
      }},
      ""relationships"": {{
        ""features"": {{
          ""links"": {{
            ""self"": ""/tandems/{tandem.StringId}/relationships/features"",
            ""related"": ""/tandems/{tandem.StringId}/features""
          }},
          ""data"": [
            {{
              ""type"": ""genericFeatures"",
              ""id"": ""{tandem.Features.ElementAt(0).StringId}""
            }}
          ]
        }},
        ""cargoBox"": {{
          ""links"": {{
            ""self"": ""/tandems/{tandem.StringId}/relationships/cargoBox"",
            ""related"": ""/tandems/{tandem.StringId}/cargoBox""
          }},
          ""data"": {{
            ""type"": ""boxes"",
            ""id"": ""{tandem.CargoBox.StringId}""
          }}
        }},
        ""lights"": {{
          ""links"": {{
            ""self"": ""/tandems/{tandem.StringId}/relationships/lights"",
            ""related"": ""/tandems/{tandem.StringId}/lights""
          }},
          ""data"": [
            {{
              ""type"": ""bicycleLights"",
              ""id"": ""{tandem.Lights.ElementAt(0).StringId}""
            }}
          ]
        }},
        ""manufacturer"": {{
          ""links"": {{
            ""self"": ""/tandems/{tandem.StringId}/relationships/manufacturer"",
            ""related"": ""/tandems/{tandem.StringId}/manufacturer""
          }},
          ""data"": {{
            ""type"": ""vehicleManufacturers"",
            ""id"": ""{tandem.Manufacturer.StringId}""
          }}
        }},
        ""wheels"": {{
          ""links"": {{
            ""self"": ""/tandems/{tandem.StringId}/relationships/wheels"",
            ""related"": ""/tandems/{tandem.StringId}/wheels""
          }},
          ""data"": [
            {{
              ""type"": ""chromeWheels"",
              ""id"": ""{tandem.Wheels.ElementAt(0).StringId}""
            }}
          ]
        }}
      }},
      ""links"": {{
        ""self"": ""/tandems/{tandem.StringId}""
      }}
    }},
    {{
      ""type"": ""trucks"",
      ""id"": ""{truck.StringId}"",
      ""attributes"": {{
        ""loadingCapacity"": {truck.LoadingCapacity.ToString(CultureInfo.InvariantCulture)},
        ""requiresDriverLicense"": {truck.RequiresDriverLicense.ToString().ToLowerInvariant()},
        ""licensePlate"": ""{truck.LicensePlate}"",
        ""weight"": {truck.Weight.ToString(CultureInfo.InvariantCulture)}
      }},
      ""relationships"": {{
        ""sleepingArea"": {{
          ""links"": {{
            ""self"": ""/trucks/{truck.StringId}/relationships/sleepingArea"",
            ""related"": ""/trucks/{truck.StringId}/sleepingArea""
          }},
          ""data"": {{
            ""type"": ""boxes"",
            ""id"": ""{truck.SleepingArea.StringId}""
          }}
        }},
        ""features"": {{
          ""links"": {{
            ""self"": ""/trucks/{truck.StringId}/relationships/features"",
            ""related"": ""/trucks/{truck.StringId}/features""
          }},
          ""data"": [
            {{
              ""type"": ""genericFeatures"",
              ""id"": ""{truck.Features.ElementAt(0).StringId}""
            }}
          ]
        }},
        ""engine"": {{
          ""links"": {{
            ""self"": ""/trucks/{truck.StringId}/relationships/engine"",
            ""related"": ""/trucks/{truck.StringId}/engine""
          }},
          ""data"": {{
            ""type"": ""dieselEngines"",
            ""id"": ""{truck.Engine.StringId}""
          }}
        }},
        ""navigationSystem"": {{
          ""links"": {{
            ""self"": ""/trucks/{truck.StringId}/relationships/navigationSystem"",
            ""related"": ""/trucks/{truck.StringId}/navigationSystem""
          }},
          ""data"": {{
            ""type"": ""navigationSystems"",
            ""id"": ""{truck.NavigationSystem.StringId}""
          }}
        }},
        ""manufacturer"": {{
          ""links"": {{
            ""self"": ""/trucks/{truck.StringId}/relationships/manufacturer"",
            ""related"": ""/trucks/{truck.StringId}/manufacturer""
          }},
          ""data"": {{
            ""type"": ""vehicleManufacturers"",
            ""id"": ""{truck.Manufacturer.StringId}""
          }}
        }},
        ""wheels"": {{
          ""links"": {{
            ""self"": ""/trucks/{truck.StringId}/relationships/wheels"",
            ""related"": ""/trucks/{truck.StringId}/wheels""
          }},
          ""data"": [
            {{
              ""type"": ""chromeWheels"",
              ""id"": ""{truck.Wheels.ElementAt(0).StringId}""
            }}
          ]
        }}
      }},
      ""links"": {{
        ""self"": ""/trucks/{truck.StringId}""
      }}
    }}
  ],
  ""included"": [
    {{
      ""type"": ""boxes"",
      ""id"": ""{bike.CargoBox.StringId}"",
      ""attributes"": {{
        ""width"": {bike.CargoBox.Width.ToString(CultureInfo.InvariantCulture)},
        ""height"": {bike.CargoBox.Height.ToString(CultureInfo.InvariantCulture)},
        ""depth"": {bike.CargoBox.Depth.ToString(CultureInfo.InvariantCulture)}
      }}
    }},
    {{
      ""type"": ""bicycleLights"",
      ""id"": ""{bike.Lights.ElementAt(0).StringId}"",
      ""attributes"": {{
        ""color"": ""{bike.Lights.ElementAt(0).Color}""
      }}
    }},
    {{
      ""type"": ""vehicleManufacturers"",
      ""id"": ""{bike.Manufacturer.StringId}"",
      ""attributes"": {{
        ""name"": ""{bike.Manufacturer.Name}""
      }}
    }},
    {{
      ""type"": ""carbonWheels"",
      ""id"": ""{bike.Wheels.ElementAt(0).StringId}"",
      ""attributes"": {{
        ""hasTube"": {bike.Wheels.Cast<CarbonWheel>().ElementAt(0).HasTube.ToString().ToLowerInvariant()},
        ""radius"": {bike.Wheels.ElementAt(0).Radius.ToString(CultureInfo.InvariantCulture)}
      }},
      ""relationships"": {{
        ""vehicle"": {{
          ""links"": {{
            ""self"": ""/carbonWheels/{bike.Wheels.ElementAt(0).StringId}/relationships/vehicle"",
            ""related"": ""/carbonWheels/{bike.Wheels.ElementAt(0).StringId}/vehicle""
          }}
        }}
      }},
      ""links"": {{
        ""self"": ""/carbonWheels/{bike.Wheels.ElementAt(0).StringId}""
      }}
    }},
    {{
      ""type"": ""genericFeatures"",
      ""id"": ""{car.Features.ElementAt(0).StringId}"",
      ""attributes"": {{
        ""description"": ""{car.Features.ElementAt(0).Description}""
      }},
      ""relationships"": {{
        ""properties"": {{
          ""data"": [
            {{
              ""type"": ""numberProperties"",
              ""id"": ""{car.Features.ElementAt(0).Properties.ElementAt(0).StringId}""
            }}
          ]
        }}
      }}
    }},
    {{
      ""type"": ""numberProperties"",
      ""id"": ""{car.Features.ElementAt(0).Properties.ElementAt(0).StringId}"",
      ""attributes"": {{
        ""name"": ""{car.Features.ElementAt(0).Properties.ElementAt(0).Name}""
      }},
      ""relationships"": {{
        ""value"": {{
          ""data"": {{
            ""type"": ""numberValues"",
            ""id"": ""{car.Features.ElementAt(0).Properties.OfType<NumberProperty>().ElementAt(0).Value.StringId}""
          }}
        }}
      }}
    }},
    {{
      ""type"": ""numberValues"",
      ""id"": ""{car.Features.ElementAt(0).Properties.OfType<NumberProperty>().ElementAt(0).Value.StringId}"",
      ""attributes"": {{
        ""content"": {car.Features.ElementAt(0).Properties.OfType<NumberProperty>().ElementAt(0).Value.Content.ToString(CultureInfo.InvariantCulture)}
      }}
    }},
    {{
      ""type"": ""gasolineEngines"",
      ""id"": ""{car.Engine.StringId}"",
      ""attributes"": {{
        ""isHydrocarbonBased"": {car.Engine.IsHydrocarbonBased.ToString().ToLowerInvariant()},
        ""serialCode"": ""{((GasolineEngine)car.Engine).SerialCode}"",
        ""volatility"": {((GasolineEngine)car.Engine).Volatility.ToString(CultureInfo.InvariantCulture)},
        ""capacity"": {car.Engine.Capacity.ToString(CultureInfo.InvariantCulture)}
      }},
      ""relationships"": {{
        ""cylinders"": {{
          ""links"": {{
            ""self"": ""/gasolineEngines/{car.Engine.StringId}/relationships/cylinders"",
            ""related"": ""/gasolineEngines/{car.Engine.StringId}/cylinders""
          }},
          ""data"": [
            {{
              ""type"": ""cylinders"",
              ""id"": ""{((GasolineEngine)car.Engine).Cylinders.ElementAt(0).StringId}""
            }}
          ]
        }}
      }},
      ""links"": {{
        ""self"": ""/gasolineEngines/{car.Engine.StringId}""
      }}
    }},
    {{
      ""type"": ""cylinders"",
      ""id"": ""{((GasolineEngine)car.Engine).Cylinders.ElementAt(0).StringId}"",
      ""attributes"": {{
        ""sparkPlugCount"": {((GasolineEngine)car.Engine).Cylinders.ElementAt(0).SparkPlugCount}
      }}
    }},
    {{
      ""type"": ""navigationSystems"",
      ""id"": ""{car.NavigationSystem.StringId}"",
      ""attributes"": {{
        ""modelType"": ""{car.NavigationSystem.ModelType}""
      }}
    }},
    {{
      ""type"": ""vehicleManufacturers"",
      ""id"": ""{car.Manufacturer.StringId}"",
      ""attributes"": {{
        ""name"": ""{car.Manufacturer.Name}""
      }}
    }},
    {{
      ""type"": ""carbonWheels"",
      ""id"": ""{car.Wheels.ElementAt(0).StringId}"",
      ""attributes"": {{
        ""hasTube"": {car.Wheels.OfType<CarbonWheel>().ElementAt(0).HasTube.ToString().ToLowerInvariant()},
        ""radius"": {car.Wheels.ElementAt(0).Radius.ToString(CultureInfo.InvariantCulture)}
      }},
      ""relationships"": {{
        ""vehicle"": {{
          ""links"": {{
            ""self"": ""/carbonWheels/{car.Wheels.ElementAt(0).StringId}/relationships/vehicle"",
            ""related"": ""/carbonWheels/{car.Wheels.ElementAt(0).StringId}/vehicle""
          }}
        }}
      }},
      ""links"": {{
        ""self"": ""/carbonWheels/{car.Wheels.ElementAt(0).StringId}""
      }}
    }},
    {{
      ""type"": ""genericFeatures"",
      ""id"": ""{tandem.Features.ElementAt(0).StringId}"",
      ""attributes"": {{
        ""description"": ""{tandem.Features.ElementAt(0).Description}""
      }},
      ""relationships"": {{
        ""properties"": {{
          ""data"": [
            {{
              ""type"": ""stringProperties"",
              ""id"": ""{tandem.Features.ElementAt(0).Properties.ElementAt(0).StringId}""
            }}
          ]
        }}
      }}
    }},
    {{
      ""type"": ""stringProperties"",
      ""id"": ""{tandem.Features.ElementAt(0).Properties.ElementAt(0).StringId}"",
      ""attributes"": {{
        ""name"": ""{tandem.Features.ElementAt(0).Properties.ElementAt(0).Name}""
      }},
      ""relationships"": {{
        ""value"": {{
          ""data"": {{
            ""type"": ""stringValues"",
            ""id"": ""{tandem.Features.ElementAt(0).Properties.OfType<StringProperty>().ElementAt(0).Value.StringId}""
          }}
        }}
      }}
    }},
    {{
      ""type"": ""stringValues"",
      ""id"": ""{tandem.Features.ElementAt(0).Properties.OfType<StringProperty>().ElementAt(0).Value.StringId}"",
      ""attributes"": {{
        ""content"": ""{tandem.Features.ElementAt(0).Properties.OfType<StringProperty>().ElementAt(0).Value.Content}""
      }}
    }},
    {{
      ""type"": ""boxes"",
      ""id"": ""{tandem.CargoBox.StringId}"",
      ""attributes"": {{
        ""width"": {tandem.CargoBox.Width.ToString(CultureInfo.InvariantCulture)},
        ""height"": {tandem.CargoBox.Height.ToString(CultureInfo.InvariantCulture)},
        ""depth"": {tandem.CargoBox.Depth.ToString(CultureInfo.InvariantCulture)}
      }}
    }},
    {{
      ""type"": ""bicycleLights"",
      ""id"": ""{tandem.Lights.ElementAt(0).StringId}"",
      ""attributes"": {{
        ""color"": ""{tandem.Lights.ElementAt(0).Color}""
      }}
    }},
    {{
      ""type"": ""vehicleManufacturers"",
      ""id"": ""{tandem.Manufacturer.StringId}"",
      ""attributes"": {{
        ""name"": ""{tandem.Manufacturer.Name}""
      }}
    }},
    {{
      ""type"": ""chromeWheels"",
      ""id"": ""{tandem.Wheels.ElementAt(0).StringId}"",
      ""attributes"": {{
        ""paintColor"": ""{tandem.Wheels.OfType<ChromeWheel>().ElementAt(0).PaintColor}"",
        ""radius"": {tandem.Wheels.ElementAt(0).Radius.ToString(CultureInfo.InvariantCulture)}
      }},
      ""relationships"": {{
        ""vehicle"": {{
          ""links"": {{
            ""self"": ""/chromeWheels/{tandem.Wheels.ElementAt(0).StringId}/relationships/vehicle"",
            ""related"": ""/chromeWheels/{tandem.Wheels.ElementAt(0).StringId}/vehicle""
          }}
        }}
      }},
      ""links"": {{
        ""self"": ""/chromeWheels/{tandem.Wheels.ElementAt(0).StringId}""
      }}
    }},
    {{
      ""type"": ""boxes"",
      ""id"": ""{truck.SleepingArea.StringId}"",
      ""attributes"": {{
        ""width"": {truck.SleepingArea.Width.ToString(CultureInfo.InvariantCulture)},
        ""height"": {truck.SleepingArea.Height.ToString(CultureInfo.InvariantCulture)},
        ""depth"": {truck.SleepingArea.Depth.ToString(CultureInfo.InvariantCulture)}
      }}
    }},
    {{
      ""type"": ""genericFeatures"",
      ""id"": ""{truck.Features.ElementAt(0).StringId}"",
      ""attributes"": {{
        ""description"": ""{truck.Features.ElementAt(0).Description}""
      }},
      ""relationships"": {{
        ""properties"": {{
          ""data"": [
            {{
              ""type"": ""stringProperties"",
              ""id"": ""{truck.Features.ElementAt(0).Properties.ElementAt(0).StringId}""
            }}
          ]
        }}
      }}
    }},
    {{
      ""type"": ""stringProperties"",
      ""id"": ""{truck.Features.ElementAt(0).Properties.ElementAt(0).StringId}"",
      ""attributes"": {{
        ""name"": ""{truck.Features.ElementAt(0).Properties.ElementAt(0).Name}""
      }},
      ""relationships"": {{
        ""value"": {{
          ""data"": {{
            ""type"": ""stringValues"",
            ""id"": ""{truck.Features.ElementAt(0).Properties.OfType<StringProperty>().ElementAt(0).Value.StringId}""
          }}
        }}
      }}
    }},
    {{
      ""type"": ""stringValues"",
      ""id"": ""{truck.Features.ElementAt(0).Properties.OfType<StringProperty>().ElementAt(0).Value.StringId}"",
      ""attributes"": {{
        ""content"": ""{truck.Features.ElementAt(0).Properties.OfType<StringProperty>().ElementAt(0).Value.Content}""
      }}
    }},
    {{
      ""type"": ""dieselEngines"",
      ""id"": ""{truck.Engine.StringId}"",
      ""attributes"": {{
        ""isHydrocarbonBased"": {truck.Engine.IsHydrocarbonBased.ToString().ToLowerInvariant()},
        ""serialCode"": ""{((DieselEngine)truck.Engine).SerialCode}"",
        ""viscosity"": {((DieselEngine)truck.Engine).Viscosity.ToString(CultureInfo.InvariantCulture)},
        ""capacity"": {truck.Engine.Capacity.ToString(CultureInfo.InvariantCulture)}
      }},
      ""links"": {{
        ""self"": ""/dieselEngines/{truck.Engine.StringId}""
      }}
    }},
    {{
      ""type"": ""navigationSystems"",
      ""id"": ""{truck.NavigationSystem.StringId}"",
      ""attributes"": {{
        ""modelType"": ""{truck.NavigationSystem.ModelType}""
      }}
    }},
    {{
      ""type"": ""vehicleManufacturers"",
      ""id"": ""{truck.Manufacturer.StringId}"",
      ""attributes"": {{
        ""name"": ""{truck.Manufacturer.Name}""
      }}
    }},
    {{
      ""type"": ""chromeWheels"",
      ""id"": ""{truck.Wheels.ElementAt(0).StringId}"",
      ""attributes"": {{
        ""paintColor"": ""{truck.Wheels.OfType<ChromeWheel>().ElementAt(0).PaintColor}"",
        ""radius"": {truck.Wheels.ElementAt(0).Radius.ToString(CultureInfo.InvariantCulture)}
      }},
      ""relationships"": {{
        ""vehicle"": {{
          ""links"": {{
            ""self"": ""/chromeWheels/{truck.Wheels.ElementAt(0).StringId}/relationships/vehicle"",
            ""related"": ""/chromeWheels/{truck.Wheels.ElementAt(0).StringId}/vehicle""
          }}
        }}
      }},
      ""links"": {{
        ""self"": ""/chromeWheels/{truck.Wheels.ElementAt(0).StringId}""
      }}
    }}
  ]
}}");
    }

    [Fact]
    public async Task Can_filter_on_derived_resource_type_at_abstract_endpoint()
    {
        // Arrange
        Bike bike = _fakers.Bike.Generate();

        Tandem tandem = _fakers.Tandem.Generate();

        Car car = _fakers.Car.Generate();
        car.Engine = _fakers.GasolineEngine.Generate();

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
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(2);

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "bikes" && resource.Id == bike.StringId);
        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "tandems" && resource.Id == tandem.StringId);
    }

    [Fact]
    public async Task Can_filter_on_derived_resource_type_with_condition_at_abstract_endpoint()
    {
        // Arrange
        Bike bike = _fakers.Bike.Generate();

        Car car = _fakers.Car.Generate();
        car.LicensePlate = "XX-99-YY";
        car.Engine = _fakers.GasolineEngine.Generate();

        Truck truck = _fakers.Truck.Generate();
        truck.LicensePlate = "AA-11-BB";
        truck.Engine = _fakers.DieselEngine.Generate();

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
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "trucks" && resource.Id == truck.StringId);
    }

    [Fact]
    public async Task Can_filter_on_derived_resource_type_and_derived_ToOne_relationship_type_at_abstract_endpoint()
    {
        // Arrange
        Bike bike = _fakers.Bike.Generate();

        Car car = _fakers.Car.Generate();
        car.Engine = _fakers.GasolineEngine.Generate();

        Truck truck = _fakers.Truck.Generate();
        truck.Engine = _fakers.DieselEngine.Generate();

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
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "trucks" && resource.Id == truck.StringId);
    }

    [Fact]
    public async Task Can_filter_on_derived_resource_type_and_derived_ToOne_relationship_type_with_condition_at_abstract_endpoint()
    {
        // Arrange
        Car car = _fakers.Car.Generate();
        car.Engine = _fakers.GasolineEngine.Generate();

        Truck truck1 = _fakers.Truck.Generate();
        truck1.Engine = _fakers.DieselEngine.Generate();
        ((DieselEngine)truck1.Engine).Viscosity = 25;

        Truck truck2 = _fakers.Truck.Generate();
        truck2.Engine = _fakers.DieselEngine.Generate();
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
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "trucks" && resource.Id == truck2.StringId);
    }

    [Fact]
    public async Task Can_filter_on_derived_resource_type_with_condition_at_concrete_base_endpoint()
    {
        // Arrange
        Bike bike = _fakers.Bike.Generate();

        Tandem tandem1 = _fakers.Tandem.Generate();

        Tandem tandem2 = _fakers.Tandem.Generate();
        tandem2.Features = _fakers.GenericFeature.Generate(1).ToHashSet();

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
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "tandems" && resource.Id == tandem2.StringId);
    }

    [Fact]
    public async Task Can_filter_on_derived_resource_type_with_condition_at_concrete_derived_endpoint()
    {
        // Arrange
        Car car1 = _fakers.Car.Generate();
        car1.Engine = _fakers.GasolineEngine.Generate();
        car1.Wheels = _fakers.CarbonWheel.Generate(4).Cast<Wheel>().ToHashSet();

        Car car2 = _fakers.Car.Generate();
        car2.Engine = _fakers.GasolineEngine.Generate();
        car2.Wheels = _fakers.ChromeWheel.Generate(4).Cast<Wheel>().ToHashSet();

        Car car3 = _fakers.Car.Generate();
        car3.Engine = _fakers.GasolineEngine.Generate();
        car3.Wheels = _fakers.ChromeWheel.Generate(4).Cast<Wheel>().ToHashSet();
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
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "cars" && resource.Id == car3.StringId);
    }
}
