using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance;

public abstract class ResourceInheritanceWriteTests<TDbContext> : IClassFixture<IntegrationTestContext<TestableStartup<TDbContext>, TDbContext>>
    where TDbContext : ResourceInheritanceDbContext
{
    private readonly IntegrationTestContext<TestableStartup<TDbContext>, TDbContext> _testContext;
    private readonly ResourceInheritanceFakers _fakers = new();

    protected ResourceInheritanceWriteTests(IntegrationTestContext<TestableStartup<TDbContext>, TDbContext> testContext)
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

        testContext.UseController<VehicleManufacturersController>();

        testContext.ConfigureServicesAfterStartup(services =>
        {
            services.AddSingleton<ResourceTypeCaptureStore<Bike, long>>();
            services.AddResourceDefinition<ResourceTypeCapturingDefinition<Bike, long>>();

            services.AddSingleton<ResourceTypeCaptureStore<Tandem, long>>();
            services.AddResourceDefinition<ResourceTypeCapturingDefinition<Tandem, long>>();

            services.AddSingleton<ResourceTypeCaptureStore<Car, long>>();
            services.AddResourceDefinition<ResourceTypeCapturingDefinition<Car, long>>();

            services.AddSingleton<ResourceTypeCaptureStore<CarbonWheel, long>>();
            services.AddResourceDefinition<ResourceTypeCapturingDefinition<CarbonWheel, long>>();

            services.AddSingleton<ResourceTypeCaptureStore<VehicleManufacturer, long>>();
            services.AddResourceDefinition<ResourceTypeCapturingDefinition<VehicleManufacturer, long>>();
        });

        var bikeStore = _testContext.Factory.Services.GetRequiredService<ResourceTypeCaptureStore<Bike, long>>();
        bikeStore.Reset();

        var tandemStore = _testContext.Factory.Services.GetRequiredService<ResourceTypeCaptureStore<Tandem, long>>();
        tandemStore.Reset();

        var carStore = _testContext.Factory.Services.GetRequiredService<ResourceTypeCaptureStore<Car, long>>();
        carStore.Reset();

        var carbonWheelStore = _testContext.Factory.Services.GetRequiredService<ResourceTypeCaptureStore<CarbonWheel, long>>();
        carbonWheelStore.Reset();

        var manufacturerStore = _testContext.Factory.Services.GetRequiredService<ResourceTypeCaptureStore<VehicleManufacturer, long>>();
        manufacturerStore.Reset();
    }

    [Fact]
    public async Task Cannot_create_abstract_resource_at_abstract_endpoint()
    {
        // Arrange
        Tandem newTandem = _fakers.Tandem.Generate();

        var requestBody = new
        {
            data = new
            {
                type = "vehicles",
                attributes = new
                {
                    weight = newTandem.Weight
                }
            }
        };

        const string route = "/vehicles";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Abstract resource type found.");
        error.Detail.Should().Be("Resource type 'vehicles' is abstract.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data/type");
    }

    [Fact]
    public async Task Can_create_concrete_base_resource_at_abstract_endpoint_with_relationships()
    {
        // Arrange
        var bikeStore = _testContext.Factory.Services.GetRequiredService<ResourceTypeCaptureStore<Bike, long>>();

        VehicleManufacturer existingManufacturer = _fakers.VehicleManufacturer.Generate();
        ChromeWheel existingChromeWheel = _fakers.ChromeWheel.Generate();
        Box existingBox = _fakers.Box.Generate();
        BicycleLight existingLight = _fakers.BicycleLight.Generate();

        Bike newBike = _fakers.Bike.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.VehicleManufacturers.Add(existingManufacturer);
            dbContext.Wheels.Add(existingChromeWheel);
            dbContext.Boxes.Add(existingBox);
            dbContext.BicycleLights.Add(existingLight);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "bikes",
                attributes = new
                {
                    weight = newBike.Weight,
                    requiresDriverLicense = newBike.RequiresDriverLicense,
                    gearCount = newBike.GearCount
                },
                relationships = new
                {
                    manufacturer = new
                    {
                        data = new
                        {
                            type = "vehicleManufacturers",
                            id = existingManufacturer.StringId
                        }
                    },
                    wheels = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "chromeWheels",
                                id = existingChromeWheel.StringId
                            }
                        }
                    },
                    cargoBox = new
                    {
                        data = new
                        {
                            type = "boxes",
                            id = existingBox.StringId
                        }
                    },
                    lights = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "bicycleLights",
                                id = existingLight.StringId
                            }
                        }
                    }
                }
            }
        };

        const string route = "/vehicles";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("bikes");
        responseDocument.Data.SingleValue.Attributes.ShouldOnlyContainKeys("weight", "requiresDriverLicense", "gearCount");
        responseDocument.Data.SingleValue.Relationships.ShouldOnlyContainKeys("manufacturer", "wheels", "cargoBox", "lights");

        long newBikeId = long.Parse(responseDocument.Data.SingleValue.Id.ShouldNotBeNull());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:keep_existing_linebreaks true

            Bike bikeInDatabase = await dbContext.Bikes
                .Include(tandem => tandem.Manufacturer)
                .Include(tandem => tandem.Wheels)
                .Include(tandem => tandem.CargoBox)
                .Include(tandem => tandem.Lights)
                .FirstWithIdAsync(newBikeId);

            // @formatter:keep_existing_linebreaks restore
            // @formatter:wrap_chained_method_calls restore

            bikeInDatabase.Should().BeOfType<Bike>();
            bikeInDatabase.Weight.Should().Be(newBike.Weight);
            bikeInDatabase.RequiresDriverLicense.Should().Be(newBike.RequiresDriverLicense);
            bikeInDatabase.GearCount.Should().Be(newBike.GearCount);

            bikeInDatabase.Manufacturer.ShouldNotBeNull();
            bikeInDatabase.Manufacturer.Id.Should().Be(existingManufacturer.Id);

            bikeInDatabase.Wheels.ShouldHaveCount(1);
            bikeInDatabase.Wheels.ElementAt(0).Should().BeOfType<ChromeWheel>();
            bikeInDatabase.Wheels.ElementAt(0).Id.Should().Be(existingChromeWheel.Id);

            bikeInDatabase.CargoBox.ShouldNotBeNull();
            bikeInDatabase.CargoBox.Id.Should().Be(existingBox.Id);

            bikeInDatabase.Lights.ShouldHaveCount(1);
            bikeInDatabase.Lights.ElementAt(0).Id.Should().Be(existingLight.Id);
        });

        bikeStore.AssertLeftType<Bike>();
        bikeStore.AssertRightTypes(typeof(VehicleManufacturer), typeof(ChromeWheel), typeof(Box), typeof(BicycleLight));
    }

    [Fact]
    public async Task Can_create_concrete_derived_resource_at_abstract_endpoint_with_relationships()
    {
        // Arrange
        var carStore = _testContext.Factory.Services.GetRequiredService<ResourceTypeCaptureStore<Car, long>>();

        VehicleManufacturer existingManufacturer = _fakers.VehicleManufacturer.Generate();
        CarbonWheel existingCarbonWheel = _fakers.CarbonWheel.Generate();
        GasolineEngine existingGasolineEngine = _fakers.GasolineEngine.Generate();
        NavigationSystem existingNavigationSystem = _fakers.NavigationSystem.Generate();
        GenericFeature existingFeature = _fakers.GenericFeature.Generate();

        Car newCar = _fakers.Car.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.VehicleManufacturers.Add(existingManufacturer);
            dbContext.Wheels.Add(existingCarbonWheel);
            dbContext.Engines.Add(existingGasolineEngine);
            dbContext.NavigationSystems.Add(existingNavigationSystem);
            dbContext.GenericFeatures.Add(existingFeature);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "cars",
                attributes = new
                {
                    weight = newCar.Weight,
                    requiresDriverLicense = newCar.RequiresDriverLicense,
                    licensePlate = newCar.LicensePlate,
                    seatCount = newCar.SeatCount
                },
                relationships = new
                {
                    manufacturer = new
                    {
                        data = new
                        {
                            type = "vehicleManufacturers",
                            id = existingManufacturer.StringId
                        }
                    },
                    wheels = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "carbonWheels",
                                id = existingCarbonWheel.StringId
                            }
                        }
                    },
                    engine = new
                    {
                        data = new
                        {
                            type = "gasolineEngines",
                            id = existingGasolineEngine.StringId
                        }
                    },
                    navigationSystem = new
                    {
                        data = new
                        {
                            type = "navigationSystems",
                            id = existingNavigationSystem.StringId
                        }
                    },
                    features = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "genericFeatures",
                                id = existingFeature.StringId
                            }
                        }
                    }
                }
            }
        };

        const string route = "/vehicles";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("cars");
        responseDocument.Data.SingleValue.Attributes.ShouldOnlyContainKeys("weight", "requiresDriverLicense", "licensePlate", "seatCount");
        responseDocument.Data.SingleValue.Relationships.ShouldOnlyContainKeys("manufacturer", "wheels", "engine", "navigationSystem", "features");

        long newCarId = long.Parse(responseDocument.Data.SingleValue.Id.ShouldNotBeNull());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:keep_existing_linebreaks true

            Car carInDatabase = await dbContext.Cars
                .Include(car => car.Manufacturer)
                .Include(car => car.Wheels)
                .Include(car => car.Engine)
                .Include(car => car.NavigationSystem)
                .Include(car => car.Features)
                .FirstWithIdAsync(newCarId);

            // @formatter:keep_existing_linebreaks restore
            // @formatter:wrap_chained_method_calls restore

            carInDatabase.Should().BeOfType<Car>();
            carInDatabase.Weight.Should().Be(newCar.Weight);
            carInDatabase.RequiresDriverLicense.Should().Be(newCar.RequiresDriverLicense);
            carInDatabase.LicensePlate.Should().Be(newCar.LicensePlate);
            carInDatabase.SeatCount.Should().Be(newCar.SeatCount);

            carInDatabase.Manufacturer.ShouldNotBeNull();
            carInDatabase.Manufacturer.Id.Should().Be(existingManufacturer.Id);

            carInDatabase.Wheels.ShouldHaveCount(1);
            carInDatabase.Wheels.ElementAt(0).Should().BeOfType<CarbonWheel>();
            carInDatabase.Wheels.ElementAt(0).Id.Should().Be(existingCarbonWheel.Id);

            carInDatabase.Engine.ShouldNotBeNull();
            carInDatabase.Engine.Should().BeOfType<GasolineEngine>();
            carInDatabase.Engine.Id.Should().Be(existingGasolineEngine.Id);

            carInDatabase.NavigationSystem.ShouldNotBeNull();
            carInDatabase.NavigationSystem.Id.Should().Be(existingNavigationSystem.Id);

            carInDatabase.Features.ShouldHaveCount(1);
            carInDatabase.Features.ElementAt(0).Id.Should().Be(existingFeature.Id);
        });

        carStore.AssertLeftType<Car>();
        carStore.AssertRightTypes(typeof(VehicleManufacturer), typeof(CarbonWheel), typeof(GasolineEngine), typeof(NavigationSystem), typeof(GenericFeature));
    }

    [Fact]
    public async Task Can_create_concrete_derived_resource_at_concrete_base_endpoint_with_relationships()
    {
        // Arrange
        var tandemStore = _testContext.Factory.Services.GetRequiredService<ResourceTypeCaptureStore<Tandem, long>>();

        VehicleManufacturer existingManufacturer = _fakers.VehicleManufacturer.Generate();
        ChromeWheel existingChromeWheel = _fakers.ChromeWheel.Generate();
        Box existingBox = _fakers.Box.Generate();
        BicycleLight existingLight = _fakers.BicycleLight.Generate();
        GenericFeature existingFeature = _fakers.GenericFeature.Generate();

        Tandem newTandem = _fakers.Tandem.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.VehicleManufacturers.Add(existingManufacturer);
            dbContext.Wheels.Add(existingChromeWheel);
            dbContext.Boxes.Add(existingBox);
            dbContext.BicycleLights.Add(existingLight);
            dbContext.GenericFeatures.Add(existingFeature);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "tandems",
                attributes = new
                {
                    weight = newTandem.Weight,
                    requiresDriverLicense = newTandem.RequiresDriverLicense,
                    gearCount = newTandem.GearCount,
                    passengerCount = newTandem.PassengerCount
                },
                relationships = new
                {
                    manufacturer = new
                    {
                        data = new
                        {
                            type = "vehicleManufacturers",
                            id = existingManufacturer.StringId
                        }
                    },
                    wheels = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "chromeWheels",
                                id = existingChromeWheel.StringId
                            }
                        }
                    },
                    cargoBox = new
                    {
                        data = new
                        {
                            type = "boxes",
                            id = existingBox.StringId
                        }
                    },
                    lights = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "bicycleLights",
                                id = existingLight.StringId
                            }
                        }
                    },
                    features = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "genericFeatures",
                                id = existingFeature.StringId
                            }
                        }
                    }
                }
            }
        };

        const string route = "/bikes";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("tandems");
        responseDocument.Data.SingleValue.Attributes.ShouldOnlyContainKeys("weight", "requiresDriverLicense", "gearCount", "passengerCount");
        responseDocument.Data.SingleValue.Relationships.ShouldOnlyContainKeys("manufacturer", "wheels", "cargoBox", "lights", "features");

        long newTandemId = long.Parse(responseDocument.Data.SingleValue.Id.ShouldNotBeNull());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:keep_existing_linebreaks true

            Tandem tandemInDatabase = await dbContext.Tandems
                .Include(tandem => tandem.Manufacturer)
                .Include(tandem => tandem.Wheels)
                .Include(tandem => tandem.CargoBox)
                .Include(tandem => tandem.Lights)
                .Include(tandem => tandem.Features)
                .FirstWithIdAsync(newTandemId);

            // @formatter:keep_existing_linebreaks restore
            // @formatter:wrap_chained_method_calls restore

            tandemInDatabase.Should().BeOfType<Tandem>();
            tandemInDatabase.Weight.Should().Be(newTandem.Weight);
            tandemInDatabase.RequiresDriverLicense.Should().Be(newTandem.RequiresDriverLicense);
            tandemInDatabase.GearCount.Should().Be(newTandem.GearCount);
            tandemInDatabase.PassengerCount.Should().Be(newTandem.PassengerCount);

            tandemInDatabase.Manufacturer.ShouldNotBeNull();
            tandemInDatabase.Manufacturer.Id.Should().Be(existingManufacturer.Id);

            tandemInDatabase.Wheels.ShouldHaveCount(1);
            tandemInDatabase.Wheels.ElementAt(0).Should().BeOfType<ChromeWheel>();
            tandemInDatabase.Wheels.ElementAt(0).Id.Should().Be(existingChromeWheel.Id);

            tandemInDatabase.CargoBox.ShouldNotBeNull();
            tandemInDatabase.CargoBox.Id.Should().Be(existingBox.Id);

            tandemInDatabase.Lights.ShouldHaveCount(1);
            tandemInDatabase.Lights.ElementAt(0).Id.Should().Be(existingLight.Id);

            tandemInDatabase.Features.ShouldHaveCount(1);
            tandemInDatabase.Features.ElementAt(0).Id.Should().Be(existingFeature.Id);
        });

        tandemStore.AssertLeftType<Tandem>();
        tandemStore.AssertRightTypes(typeof(VehicleManufacturer), typeof(ChromeWheel), typeof(Box), typeof(BicycleLight), typeof(GenericFeature));
    }

    [Fact]
    public async Task Cannot_create_concrete_base_resource_at_concrete_derived_endpoint()
    {
        // Arrange
        Bike newBike = _fakers.Bike.Generate();

        var requestBody = new
        {
            data = new
            {
                type = "bikes",
                attributes = new
                {
                    weight = newBike.Weight,
                    requiresDriverLicense = newBike.RequiresDriverLicense,
                    gearCount = newBike.GearCount
                }
            }
        };

        const string route = "/tandems";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().Be("Failed to deserialize request body: Incompatible resource type found.");
        error.Detail.Should().Be("Type 'bikes' is not convertible to type 'tandems'.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data/type");
    }

    [Fact]
    public async Task Cannot_create_resource_with_abstract_relationship_type()
    {
        // Arrange
        DieselEngine existingEngine = _fakers.DieselEngine.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Engines.Add(existingEngine);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "trucks",
                relationships = new
                {
                    engine = new
                    {
                        data = new
                        {
                            type = "engines",
                            id = existingEngine.StringId
                        }
                    }
                }
            }
        };

        const string route = "/trucks";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Abstract resource type found.");
        error.Detail.Should().Be("Resource type 'engines' is abstract.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data/relationships/engine/data/type");
    }

    [Fact]
    public async Task Can_create_resource_with_concrete_base_relationship_type_stored_as_derived_type()
    {
        // Arrange
        var carbonWheelStore = _testContext.Factory.Services.GetRequiredService<ResourceTypeCaptureStore<CarbonWheel, long>>();

        Tandem existingTandem = _fakers.Tandem.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(existingTandem);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "carbonWheels",
                relationships = new
                {
                    vehicle = new
                    {
                        data = new
                        {
                            type = "bikes",
                            id = existingTandem.StringId
                        }
                    }
                }
            }
        };

        const string route = "/carbonWheels";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("carbonWheels");
        responseDocument.Data.SingleValue.Attributes.ShouldNotBeEmpty();
        responseDocument.Data.SingleValue.Relationships.ShouldNotBeEmpty();

        long newCarbonWheelId = long.Parse(responseDocument.Data.SingleValue.Id.ShouldNotBeNull());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            CarbonWheel carbonWheelInDatabase = await dbContext.CarbonWheels.Include(wheel => wheel.Vehicle).FirstWithIdAsync(newCarbonWheelId);

            carbonWheelInDatabase.Should().BeOfType<CarbonWheel>();
            carbonWheelInDatabase.Vehicle.ShouldNotBeNull();
            carbonWheelInDatabase.Vehicle.Should().BeOfType<Tandem>();
            carbonWheelInDatabase.Vehicle.Id.Should().Be(existingTandem.Id);
        });

        carbonWheelStore.AssertLeftType<CarbonWheel>();
        carbonWheelStore.AssertRightTypes(typeof(Tandem));
    }

    [Fact]
    public async Task Can_create_resource_with_concrete_base_ToOne_relationship_type_stored_as_derived_type_at_resource_endpoint()
    {
        // Arrange
        var carbonWheelStore = _testContext.Factory.Services.GetRequiredService<ResourceTypeCaptureStore<CarbonWheel, long>>();

        Tandem existingTandem = _fakers.Tandem.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(existingTandem);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "carbonWheels",
                relationships = new
                {
                    vehicle = new
                    {
                        data = new
                        {
                            type = "bikes",
                            id = existingTandem.StringId
                        }
                    }
                }
            }
        };

        const string route = "/carbonWheels";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("carbonWheels");
        responseDocument.Data.SingleValue.Attributes.ShouldNotBeEmpty();
        responseDocument.Data.SingleValue.Relationships.ShouldNotBeEmpty();

        long newCarbonWheelId = long.Parse(responseDocument.Data.SingleValue.Id.ShouldNotBeNull());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            CarbonWheel carbonWheelInDatabase = await dbContext.CarbonWheels.Include(wheel => wheel.Vehicle).FirstWithIdAsync(newCarbonWheelId);

            carbonWheelInDatabase.Should().BeOfType<CarbonWheel>();
            carbonWheelInDatabase.Vehicle.ShouldNotBeNull();
            carbonWheelInDatabase.Vehicle.Should().BeOfType<Tandem>();
            carbonWheelInDatabase.Vehicle.Id.Should().Be(existingTandem.Id);
        });

        carbonWheelStore.AssertLeftType<CarbonWheel>();
        carbonWheelStore.AssertRightTypes(typeof(Tandem));
    }

    [Fact]
    public async Task Can_create_resource_with_concrete_base_ToMany_relationship_type_stored_as_derived_type_at_resource_endpoint()
    {
        // Arrange
        var manufacturerStore = _testContext.Factory.Services.GetRequiredService<ResourceTypeCaptureStore<VehicleManufacturer, long>>();

        Tandem existingTandem = _fakers.Tandem.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(existingTandem);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "vehicleManufacturers",
                relationships = new
                {
                    vehicles = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "bikes",
                                id = existingTandem.StringId
                            }
                        }
                    }
                }
            }
        };

        const string route = "/vehicleManufacturers";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("vehicleManufacturers");
        responseDocument.Data.SingleValue.Attributes.ShouldNotBeEmpty();
        responseDocument.Data.SingleValue.Relationships.ShouldNotBeEmpty();

        long newManufacturerId = long.Parse(responseDocument.Data.SingleValue.Id.ShouldNotBeNull());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            VehicleManufacturer manufacturerInDatabase = await dbContext.VehicleManufacturers.Include(manufacturer => manufacturer.Vehicles)
                .FirstWithIdAsync(newManufacturerId);

            manufacturerInDatabase.Vehicles.ShouldHaveCount(1);
            manufacturerInDatabase.Vehicles.ElementAt(0).Should().BeOfType<Tandem>();
            manufacturerInDatabase.Vehicles.ElementAt(0).Id.Should().Be(existingTandem.Id);
        });

        manufacturerStore.AssertLeftType<VehicleManufacturer>();
        manufacturerStore.AssertRightTypes(typeof(Tandem));
    }

    [Fact]
    public async Task Cannot_create_resource_with_concrete_derived_ToOne_relationship_type_stored_as_sibling_derived_type_at_resource_endpoint()
    {
        // Arrange
        Tandem existingTandem = _fakers.Tandem.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(existingTandem);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "carbonWheels",
                relationships = new
                {
                    vehicle = new
                    {
                        data = new
                        {
                            type = "trucks",
                            id = existingTandem.StringId
                        }
                    }
                }
            }
        };

        const string route = "/wheels";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("A related resource does not exist.");
        error.Detail.Should().Be($"Related resource of type 'trucks' with ID '{existingTandem.StringId}' in relationship 'vehicle' does not exist.");
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_create_resource_with_concrete_derived_ToMany_relationship_type_stored_as_sibling_derived_type_at_resource_endpoint()
    {
        // Arrange
        Truck existingTruck = _fakers.Truck.Generate();
        existingTruck.Engine = _fakers.DieselEngine.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(existingTruck);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "vehicleManufacturers",
                relationships = new
                {
                    vehicles = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "cars",
                                id = existingTruck.StringId
                            }
                        }
                    }
                }
            }
        };

        const string route = "/vehicleManufacturers";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("A related resource does not exist.");
        error.Detail.Should().Be($"Related resource of type 'cars' with ID '{existingTruck.StringId}' in relationship 'vehicles' does not exist.");
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_create_resource_with_unknown_resource_in_ToOne_relationship_at_resource_endpoint()
    {
        // Arrange
        string unknownTruckId = Unknown.StringId.For<Truck, long>();

        var requestBody = new
        {
            data = new
            {
                type = "carbonWheels",
                relationships = new
                {
                    vehicle = new
                    {
                        data = new
                        {
                            type = "trucks",
                            id = unknownTruckId
                        }
                    }
                }
            }
        };

        const string route = "/wheels";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("A related resource does not exist.");
        error.Detail.Should().Be($"Related resource of type 'trucks' with ID '{unknownTruckId}' in relationship 'vehicle' does not exist.");
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_create_resource_with_unknown_resources_in_ToMany_relationship_at_resource_endpoint()
    {
        // Arrange
        string unknownTruckId = Unknown.StringId.For<Truck, long>();

        var requestBody = new
        {
            data = new
            {
                type = "vehicleManufacturers",
                relationships = new
                {
                    vehicles = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "trucks",
                                id = unknownTruckId
                            }
                        }
                    }
                }
            }
        };

        const string route = "/vehicleManufacturers";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("A related resource does not exist.");
        error.Detail.Should().Be($"Related resource of type 'trucks' with ID '{unknownTruckId}' in relationship 'vehicles' does not exist.");
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_update_abstract_resource_at_abstract_endpoint()
    {
        // Arrange
        Tandem existingTandem = _fakers.Tandem.Generate();

        int newPassengerCount = _fakers.Tandem.Generate().PassengerCount;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(existingTandem);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "vehicles",
                id = existingTandem.StringId,
                attributes = new
                {
                    passengerCount = newPassengerCount
                }
            }
        };

        string route = $"/vehicles/{existingTandem.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Abstract resource type found.");
        error.Detail.Should().Be("Resource type 'vehicles' is abstract.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data/type");
    }

    [Fact]
    public async Task Can_update_concrete_base_resource_at_abstract_endpoint_with_relationships()
    {
        // Arrange
        var bikeStore = _testContext.Factory.Services.GetRequiredService<ResourceTypeCaptureStore<Bike, long>>();

        VehicleManufacturer existingManufacturer = _fakers.VehicleManufacturer.Generate();
        ChromeWheel existingChromeWheel = _fakers.ChromeWheel.Generate();
        Box existingBox = _fakers.Box.Generate();
        BicycleLight existingLight = _fakers.BicycleLight.Generate();

        Bike existingBike = _fakers.Bike.Generate();
        Bike newBike = _fakers.Bike.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.VehicleManufacturers.Add(existingManufacturer);
            dbContext.Wheels.Add(existingChromeWheel);
            dbContext.Boxes.Add(existingBox);
            dbContext.BicycleLights.Add(existingLight);
            dbContext.Vehicles.Add(existingBike);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "bikes",
                id = existingBike.StringId,
                attributes = new
                {
                    weight = newBike.Weight,
                    requiresDriverLicense = newBike.RequiresDriverLicense,
                    gearCount = newBike.GearCount
                },
                relationships = new
                {
                    manufacturer = new
                    {
                        data = new
                        {
                            type = "vehicleManufacturers",
                            id = existingManufacturer.StringId
                        }
                    },
                    wheels = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "chromeWheels",
                                id = existingChromeWheel.StringId
                            }
                        }
                    },
                    cargoBox = new
                    {
                        data = new
                        {
                            type = "boxes",
                            id = existingBox.StringId
                        }
                    },
                    lights = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "bicycleLights",
                                id = existingLight.StringId
                            }
                        }
                    }
                }
            }
        };

        string route = $"/vehicles/{existingBike.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:keep_existing_linebreaks true

            Bike bikeInDatabase = await dbContext.Bikes
                .Include(tandem => tandem.Manufacturer)
                .Include(tandem => tandem.Wheels)
                .Include(tandem => tandem.CargoBox)
                .Include(tandem => tandem.Lights)
                .FirstWithIdAsync(existingBike.Id);

            // @formatter:keep_existing_linebreaks restore
            // @formatter:wrap_chained_method_calls restore

            bikeInDatabase.Should().BeOfType<Bike>();
            bikeInDatabase.Weight.Should().Be(newBike.Weight);
            bikeInDatabase.RequiresDriverLicense.Should().Be(newBike.RequiresDriverLicense);
            bikeInDatabase.GearCount.Should().Be(newBike.GearCount);

            bikeInDatabase.Manufacturer.ShouldNotBeNull();
            bikeInDatabase.Manufacturer.Id.Should().Be(existingManufacturer.Id);

            bikeInDatabase.Wheels.ShouldHaveCount(1);
            bikeInDatabase.Wheels.ElementAt(0).Should().BeOfType<ChromeWheel>();
            bikeInDatabase.Wheels.ElementAt(0).Id.Should().Be(existingChromeWheel.Id);

            bikeInDatabase.CargoBox.ShouldNotBeNull();
            bikeInDatabase.CargoBox.Id.Should().Be(existingBox.Id);

            bikeInDatabase.Lights.ShouldHaveCount(1);
            bikeInDatabase.Lights.ElementAt(0).Id.Should().Be(existingLight.Id);
        });

        bikeStore.AssertLeftType<Bike>();
        bikeStore.AssertRightTypes(typeof(VehicleManufacturer), typeof(ChromeWheel), typeof(Box), typeof(BicycleLight));
    }

    [Fact]
    public async Task Can_update_concrete_base_resource_stored_as_concrete_derived_at_abstract_endpoint_with_relationships()
    {
        // Arrange
        var tandemStore = _testContext.Factory.Services.GetRequiredService<ResourceTypeCaptureStore<Tandem, long>>();

        VehicleManufacturer existingManufacturer = _fakers.VehicleManufacturer.Generate();
        ChromeWheel existingChromeWheel = _fakers.ChromeWheel.Generate();
        Box existingBox = _fakers.Box.Generate();
        BicycleLight existingLight = _fakers.BicycleLight.Generate();

        Tandem existingTandem = _fakers.Tandem.Generate();
        Bike newBike = _fakers.Bike.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.VehicleManufacturers.Add(existingManufacturer);
            dbContext.Wheels.Add(existingChromeWheel);
            dbContext.Boxes.Add(existingBox);
            dbContext.BicycleLights.Add(existingLight);
            dbContext.Vehicles.Add(existingTandem);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "bikes",
                id = existingTandem.StringId,
                attributes = new
                {
                    weight = newBike.Weight,
                    requiresDriverLicense = newBike.RequiresDriverLicense,
                    gearCount = newBike.GearCount
                },
                relationships = new
                {
                    manufacturer = new
                    {
                        data = new
                        {
                            type = "vehicleManufacturers",
                            id = existingManufacturer.StringId
                        }
                    },
                    wheels = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "chromeWheels",
                                id = existingChromeWheel.StringId
                            }
                        }
                    },
                    cargoBox = new
                    {
                        data = new
                        {
                            type = "boxes",
                            id = existingBox.StringId
                        }
                    },
                    lights = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "bicycleLights",
                                id = existingLight.StringId
                            }
                        }
                    }
                }
            }
        };

        string route = $"/vehicles/{existingTandem.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:keep_existing_linebreaks true

            Tandem tandemInDatabase = await dbContext.Tandems
                .Include(tandem => tandem.Manufacturer)
                .Include(tandem => tandem.Wheels)
                .Include(tandem => tandem.CargoBox)
                .Include(tandem => tandem.Lights)
                .FirstWithIdAsync(existingTandem.Id);

            // @formatter:keep_existing_linebreaks restore
            // @formatter:wrap_chained_method_calls restore

            tandemInDatabase.Should().BeOfType<Tandem>();
            tandemInDatabase.Weight.Should().Be(newBike.Weight);
            tandemInDatabase.RequiresDriverLicense.Should().Be(newBike.RequiresDriverLicense);
            tandemInDatabase.GearCount.Should().Be(newBike.GearCount);

            tandemInDatabase.Manufacturer.ShouldNotBeNull();
            tandemInDatabase.Manufacturer.Id.Should().Be(existingManufacturer.Id);

            tandemInDatabase.Wheels.ShouldHaveCount(1);
            tandemInDatabase.Wheels.ElementAt(0).Should().BeOfType<ChromeWheel>();
            tandemInDatabase.Wheels.ElementAt(0).Id.Should().Be(existingChromeWheel.Id);

            tandemInDatabase.CargoBox.ShouldNotBeNull();
            tandemInDatabase.CargoBox.Id.Should().Be(existingBox.Id);

            tandemInDatabase.Lights.ShouldHaveCount(1);
            tandemInDatabase.Lights.ElementAt(0).Id.Should().Be(existingLight.Id);
        });

        tandemStore.AssertLeftType<Tandem>();
        tandemStore.AssertRightTypes(typeof(VehicleManufacturer), typeof(ChromeWheel), typeof(Box), typeof(BicycleLight));
    }

    [Fact]
    public async Task Cannot_update_concrete_base_resource_at_concrete_derived_endpoint()
    {
        // Arrange
        Tandem existingTandem = _fakers.Tandem.Generate();

        int newPassengerCount = _fakers.Tandem.Generate().PassengerCount;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(existingTandem);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "bikes",
                id = existingTandem.StringId,
                attributes = new
                {
                    passengerCount = newPassengerCount
                }
            }
        };

        string route = $"/tandems/{existingTandem.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().Be("Failed to deserialize request body: Incompatible resource type found.");
        error.Detail.Should().Be("Type 'bikes' is not convertible to type 'tandems'.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data/type");
    }

    [Fact]
    public async Task Cannot_update_resource_with_abstract_relationship_type()
    {
        // Arrange
        Truck existingTruck = _fakers.Truck.Generate();
        existingTruck.Engine = _fakers.GasolineEngine.Generate();

        DieselEngine existingEngine = _fakers.DieselEngine.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(existingTruck);
            dbContext.Engines.Add(existingEngine);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "trucks",
                id = existingTruck.StringId,
                relationships = new
                {
                    engine = new
                    {
                        data = new
                        {
                            type = "engines",
                            id = existingEngine.StringId
                        }
                    }
                }
            }
        };

        string route = $"/trucks/{existingTruck.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Abstract resource type found.");
        error.Detail.Should().Be("Resource type 'engines' is abstract.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data/relationships/engine/data/type");
    }

    [Fact]
    public async Task Can_update_resource_with_concrete_base_ToOne_relationship_type_stored_as_derived_type_at_resource_endpoint()
    {
        // Arrange
        var carbonWheelStore = _testContext.Factory.Services.GetRequiredService<ResourceTypeCaptureStore<CarbonWheel, long>>();

        CarbonWheel existingCarbonWheel = _fakers.CarbonWheel.Generate();

        Tandem existingTandem = _fakers.Tandem.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Wheels.Add(existingCarbonWheel);
            dbContext.Vehicles.Add(existingTandem);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "carbonWheels",
                id = existingCarbonWheel.StringId,
                relationships = new
                {
                    vehicle = new
                    {
                        data = new
                        {
                            type = "bikes",
                            id = existingTandem.StringId
                        }
                    }
                }
            }
        };

        string route = $"/carbonWheels/{existingCarbonWheel.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            CarbonWheel carbonWheelInDatabase = await dbContext.CarbonWheels.Include(wheel => wheel.Vehicle).FirstWithIdAsync(existingCarbonWheel.Id);

            carbonWheelInDatabase.Should().BeOfType<CarbonWheel>();
            carbonWheelInDatabase.Vehicle.ShouldNotBeNull();
            carbonWheelInDatabase.Vehicle.Should().BeOfType<Tandem>();
            carbonWheelInDatabase.Vehicle.Id.Should().Be(existingTandem.Id);
        });

        carbonWheelStore.AssertLeftType<CarbonWheel>();
        carbonWheelStore.AssertRightTypes(typeof(Tandem));
    }

    [Fact]
    public async Task Can_update_resource_with_concrete_base_ToMany_relationship_type_stored_as_derived_type_at_resource_endpoint()
    {
        // Arrange
        var manufacturerStore = _testContext.Factory.Services.GetRequiredService<ResourceTypeCaptureStore<VehicleManufacturer, long>>();

        VehicleManufacturer existingManufacturer = _fakers.VehicleManufacturer.Generate();

        Tandem existingTandem = _fakers.Tandem.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.VehicleManufacturers.Add(existingManufacturer);
            dbContext.Vehicles.Add(existingTandem);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "vehicleManufacturers",
                id = existingManufacturer.StringId,
                relationships = new
                {
                    vehicles = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "bikes",
                                id = existingTandem.StringId
                            }
                        }
                    }
                }
            }
        };

        string route = $"/vehicleManufacturers/{existingManufacturer.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            VehicleManufacturer manufacturerInDatabase = await dbContext.VehicleManufacturers.Include(manufacturer => manufacturer.Vehicles)
                .FirstWithIdAsync(existingManufacturer.Id);

            manufacturerInDatabase.Vehicles.ShouldHaveCount(1);
            manufacturerInDatabase.Vehicles.ElementAt(0).Should().BeOfType<Tandem>();
            manufacturerInDatabase.Vehicles.ElementAt(0).Id.Should().Be(existingTandem.Id);
        });

        manufacturerStore.AssertLeftType<VehicleManufacturer>();
        manufacturerStore.AssertRightTypes(typeof(Tandem));
    }

    [Fact]
    public async Task Cannot_update_resource_with_concrete_derived_ToOne_relationship_type_stored_as_sibling_derived_type_at_resource_endpoint()
    {
        // Arrange
        CarbonWheel existingCarbonWheel = _fakers.CarbonWheel.Generate();

        Tandem existingTandem = _fakers.Tandem.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Wheels.Add(existingCarbonWheel);
            dbContext.Vehicles.Add(existingTandem);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "carbonWheels",
                id = existingCarbonWheel.StringId,
                relationships = new
                {
                    vehicle = new
                    {
                        data = new
                        {
                            type = "trucks",
                            id = existingTandem.StringId
                        }
                    }
                }
            }
        };

        string route = $"/wheels/{existingCarbonWheel.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("A related resource does not exist.");
        error.Detail.Should().Be($"Related resource of type 'trucks' with ID '{existingTandem.StringId}' in relationship 'vehicle' does not exist.");
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_update_resource_with_concrete_derived_ToMany_relationship_type_stored_as_sibling_derived_type_at_resource_endpoint()
    {
        // Arrange
        VehicleManufacturer existingManufacturer = _fakers.VehicleManufacturer.Generate();

        Truck existingTruck = _fakers.Truck.Generate();
        existingTruck.Engine = _fakers.DieselEngine.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.VehicleManufacturers.Add(existingManufacturer);
            dbContext.Vehicles.Add(existingTruck);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "vehicleManufacturers",
                id = existingManufacturer.StringId,
                relationships = new
                {
                    vehicles = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "cars",
                                id = existingTruck.StringId
                            }
                        }
                    }
                }
            }
        };

        string route = $"/vehicleManufacturers/{existingManufacturer.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("A related resource does not exist.");
        error.Detail.Should().Be($"Related resource of type 'cars' with ID '{existingTruck.StringId}' in relationship 'vehicles' does not exist.");
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_update_resource_with_unknown_resource_in_ToOne_relationship_at_resource_endpoint()
    {
        // Arrange
        CarbonWheel existingCarbonWheel = _fakers.CarbonWheel.Generate();

        string unknownTruckId = Unknown.StringId.For<Truck, long>();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Wheels.Add(existingCarbonWheel);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "carbonWheels",
                id = existingCarbonWheel.StringId,
                relationships = new
                {
                    vehicle = new
                    {
                        data = new
                        {
                            type = "trucks",
                            id = unknownTruckId
                        }
                    }
                }
            }
        };

        string route = $"/wheels/{existingCarbonWheel.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("A related resource does not exist.");
        error.Detail.Should().Be($"Related resource of type 'trucks' with ID '{unknownTruckId}' in relationship 'vehicle' does not exist.");
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_update_resource_with_unknown_resources_in_ToMany_relationship_at_resource_endpoint()
    {
        // Arrange
        VehicleManufacturer existingManufacturer = _fakers.VehicleManufacturer.Generate();

        string unknownTruckId = Unknown.StringId.For<Truck, long>();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.VehicleManufacturers.Add(existingManufacturer);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "vehicleManufacturers",
                id = existingManufacturer.StringId,
                relationships = new
                {
                    vehicles = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "trucks",
                                id = unknownTruckId
                            }
                        }
                    }
                }
            }
        };

        string route = $"/vehicleManufacturers/{existingManufacturer.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("A related resource does not exist.");
        error.Detail.Should().Be($"Related resource of type 'trucks' with ID '{unknownTruckId}' in relationship 'vehicles' does not exist.");
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Can_delete_resource_stored_as_concrete_derived_at_abstract_endpoint()
    {
        // Arrange
        var tandemStore = _testContext.Factory.Services.GetRequiredService<ResourceTypeCaptureStore<Tandem, long>>();

        Tandem existingTandem = _fakers.Tandem.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(existingTandem);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/vehicles/{existingTandem.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Tandem? tandemInDatabase = await dbContext.Tandems.FirstWithIdOrDefaultAsync(existingTandem.Id);

            tandemInDatabase.Should().BeNull();
        });

        tandemStore.AssertLeftType<Tandem>();
        tandemStore.AssertRightTypes(Array.Empty<Type>());
    }

    [Fact]
    public async Task Cannot_delete_concrete_base_resource_at_concrete_derived_endpoint()
    {
        // Arrange
        Bike existingBike = _fakers.Bike.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(existingBike);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/tandems/{existingBike.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'tandems' with ID '{existingBike.StringId}' does not exist.");
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Can_set_abstract_resource_at_abstract_ToOne_relationship_endpoint()
    {
        // Arrange
        var carbonWheelStore = _testContext.Factory.Services.GetRequiredService<ResourceTypeCaptureStore<CarbonWheel, long>>();

        CarbonWheel existingCarbonWheel = _fakers.CarbonWheel.Generate();

        Tandem existingTandem = _fakers.Tandem.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Wheels.Add(existingCarbonWheel);
            dbContext.Vehicles.Add(existingTandem);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "vehicles",
                id = existingTandem.StringId
            }
        };

        string route = $"/wheels/{existingCarbonWheel.StringId}/relationships/vehicle";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Wheel wheelInDatabase = await dbContext.Wheels.Include(wheel => wheel.Vehicle).FirstWithIdAsync(existingCarbonWheel.Id);

            wheelInDatabase.Should().BeOfType<CarbonWheel>();

            wheelInDatabase.Vehicle.ShouldNotBeNull();
            wheelInDatabase.Vehicle.Should().BeOfType<Tandem>();
            wheelInDatabase.Vehicle.Id.Should().Be(existingTandem.Id);
        });

        carbonWheelStore.AssertLeftType<CarbonWheel>();
        carbonWheelStore.AssertRightTypes(typeof(Tandem));
    }

    [Fact]
    public async Task Can_set_abstract_resources_at_abstract_ToMany_relationship_endpoint()
    {
        // Arrange
        var tandemStore = _testContext.Factory.Services.GetRequiredService<ResourceTypeCaptureStore<Tandem, long>>();

        Tandem existingTandem = _fakers.Tandem.Generate();

        ChromeWheel existingChromeWheel = _fakers.ChromeWheel.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(existingTandem);
            dbContext.Wheels.Add(existingChromeWheel);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "wheels",
                    id = existingChromeWheel.StringId
                }
            }
        };

        string route = $"/vehicles/{existingTandem.StringId}/relationships/wheels";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Vehicle vehicleInDatabase = await dbContext.Vehicles.Include(vehicle => vehicle.Wheels).FirstWithIdAsync(existingTandem.Id);

            vehicleInDatabase.Should().BeOfType<Tandem>();

            vehicleInDatabase.Wheels.ShouldHaveCount(1);
            vehicleInDatabase.Wheels.ElementAt(0).Should().BeOfType<ChromeWheel>();
            vehicleInDatabase.Wheels.ElementAt(0).Id.Should().Be(existingChromeWheel.Id);
        });

        tandemStore.AssertLeftType<Tandem>();
        tandemStore.AssertRightTypes(typeof(ChromeWheel));
    }

    [Fact]
    public async Task Can_set_concrete_base_resource_stored_as_derived_at_abstract_ToOne_relationship_endpoint()
    {
        // Arrange
        var carbonWheelStore = _testContext.Factory.Services.GetRequiredService<ResourceTypeCaptureStore<CarbonWheel, long>>();

        CarbonWheel existingCarbonWheel = _fakers.CarbonWheel.Generate();

        Tandem existingTandem = _fakers.Tandem.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Wheels.Add(existingCarbonWheel);
            dbContext.Vehicles.Add(existingTandem);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "bikes",
                id = existingTandem.StringId
            }
        };

        string route = $"/wheels/{existingCarbonWheel.StringId}/relationships/vehicle";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Wheel wheelInDatabase = await dbContext.Wheels.Include(wheel => wheel.Vehicle).FirstWithIdAsync(existingCarbonWheel.Id);

            wheelInDatabase.Should().BeOfType<CarbonWheel>();

            wheelInDatabase.Vehicle.ShouldNotBeNull();
            wheelInDatabase.Vehicle.Should().BeOfType<Tandem>();
            wheelInDatabase.Vehicle.Id.Should().Be(existingTandem.Id);
        });

        carbonWheelStore.AssertLeftType<CarbonWheel>();
        carbonWheelStore.AssertRightTypes(typeof(Tandem));
    }

    [Fact]
    public async Task Can_set_concrete_base_resources_stored_as_derived_at_ToMany_relationship_endpoint()
    {
        // Arrange
        var manufacturerStore = _testContext.Factory.Services.GetRequiredService<ResourceTypeCaptureStore<VehicleManufacturer, long>>();

        VehicleManufacturer existingManufacturer = _fakers.VehicleManufacturer.Generate();

        Tandem existingTandem = _fakers.Tandem.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.VehicleManufacturers.Add(existingManufacturer);
            dbContext.Vehicles.Add(existingTandem);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "bikes",
                    id = existingTandem.StringId
                }
            }
        };

        string route = $"/vehicleManufacturers/{existingManufacturer.StringId}/relationships/vehicles";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            VehicleManufacturer manufacturerInDatabase = await dbContext.VehicleManufacturers.Include(manufacturer => manufacturer.Vehicles)
                .FirstWithIdAsync(existingManufacturer.Id);

            manufacturerInDatabase.Vehicles.ShouldHaveCount(1);
            manufacturerInDatabase.Vehicles.ElementAt(0).Should().BeOfType<Tandem>();
            manufacturerInDatabase.Vehicles.ElementAt(0).Id.Should().Be(existingTandem.Id);
        });

        manufacturerStore.AssertLeftType<VehicleManufacturer>();
        manufacturerStore.AssertRightTypes(typeof(Tandem));
    }

    [Fact]
    public async Task Cannot_set_concrete_derived_resource_stored_as_concrete_base_at_abstract_ToOne_relationship_endpoint()
    {
        // Arrange
        CarbonWheel existingCarbonWheel = _fakers.CarbonWheel.Generate();

        Bike existingBike = _fakers.Bike.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Wheels.Add(existingCarbonWheel);
            dbContext.Vehicles.Add(existingBike);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "tandems",
                id = existingBike.StringId
            }
        };

        string route = $"/wheels/{existingCarbonWheel.StringId}/relationships/vehicle";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("A related resource does not exist.");
        error.Detail.Should().Be($"Related resource of type 'tandems' with ID '{existingBike.StringId}' in relationship 'vehicle' does not exist.");
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_set_concrete_derived_resources_stored_as_sibling_derived_at_ToMany_relationship_endpoint()
    {
        // Arrange
        VehicleManufacturer existingManufacturer = _fakers.VehicleManufacturer.Generate();

        Truck existingTruck = _fakers.Truck.Generate();
        existingTruck.Engine = _fakers.DieselEngine.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.VehicleManufacturers.Add(existingManufacturer);
            dbContext.Vehicles.Add(existingTruck);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "cars",
                    id = existingTruck.StringId
                }
            }
        };

        string route = $"/vehicleManufacturers/{existingManufacturer.StringId}/relationships/vehicles";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("A related resource does not exist.");
        error.Detail.Should().Be($"Related resource of type 'cars' with ID '{existingTruck.StringId}' in relationship 'vehicles' does not exist.");
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_set_unknown_resources_at_ToMany_relationship_endpoint()
    {
        // Arrange
        VehicleManufacturer existingManufacturer = _fakers.VehicleManufacturer.Generate();

        string unknownTruckId = Unknown.StringId.For<Truck, long>();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.VehicleManufacturers.Add(existingManufacturer);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "trucks",
                    id = unknownTruckId
                }
            }
        };

        string route = $"/vehicleManufacturers/{existingManufacturer.StringId}/relationships/vehicles";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("A related resource does not exist.");
        error.Detail.Should().Be($"Related resource of type 'trucks' with ID '{unknownTruckId}' in relationship 'vehicles' does not exist.");
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_clear_ToOne_relationship_for_left_type_stored_as_sibling_type()
    {
        Truck existingTruck = _fakers.Truck.Generate();
        existingTruck.Engine = _fakers.DieselEngine.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(existingTruck);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = (object?)null
        };

        string route = $"/cars/{existingTruck.StringId}/relationships/engine";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'cars' with ID '{existingTruck.StringId}' does not exist.");
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Can_add_abstract_resources_at_abstract_ToMany_relationship_endpoint()
    {
        // Arrange
        var tandemStore = _testContext.Factory.Services.GetRequiredService<ResourceTypeCaptureStore<Tandem, long>>();

        Tandem existingTandem = _fakers.Tandem.Generate();
        existingTandem.Wheels = _fakers.CarbonWheel.Generate(1).Cast<Wheel>().ToHashSet();

        ChromeWheel existingChromeWheel = _fakers.ChromeWheel.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(existingTandem);
            dbContext.Wheels.Add(existingChromeWheel);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "wheels",
                    id = existingChromeWheel.StringId
                }
            }
        };

        string route = $"/vehicles/{existingTandem.StringId}/relationships/wheels";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Vehicle vehicleInDatabase = await dbContext.Vehicles.Include(vehicle => vehicle.Wheels).FirstWithIdAsync(existingTandem.Id);

            vehicleInDatabase.Should().BeOfType<Tandem>();

            vehicleInDatabase.Wheels.ShouldHaveCount(2);
            vehicleInDatabase.Wheels.ElementAt(0).Should().BeOfType<CarbonWheel>();
            vehicleInDatabase.Wheels.ElementAt(0).Id.Should().Be(existingTandem.Wheels.ElementAt(0).Id);
            vehicleInDatabase.Wheels.ElementAt(1).Should().BeOfType<ChromeWheel>();
            vehicleInDatabase.Wheels.ElementAt(1).Id.Should().Be(existingChromeWheel.Id);
        });

        tandemStore.AssertLeftType<Tandem>();
        tandemStore.AssertRightTypes(typeof(ChromeWheel));
    }

    [Fact]
    public async Task Can_add_concrete_derived_resources_at_abstract_ToMany_relationship_endpoint()
    {
        // Arrange
        var tandemStore = _testContext.Factory.Services.GetRequiredService<ResourceTypeCaptureStore<Tandem, long>>();

        Tandem existingTandem = _fakers.Tandem.Generate();
        existingTandem.Wheels = _fakers.CarbonWheel.Generate(1).Cast<Wheel>().ToHashSet();

        ChromeWheel existingChromeWheel = _fakers.ChromeWheel.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(existingTandem);
            dbContext.Wheels.Add(existingChromeWheel);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "chromeWheels",
                    id = existingChromeWheel.StringId
                }
            }
        };

        string route = $"/vehicles/{existingTandem.StringId}/relationships/wheels";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Vehicle vehicleInDatabase = await dbContext.Vehicles.Include(vehicle => vehicle.Wheels).FirstWithIdAsync(existingTandem.Id);

            vehicleInDatabase.Should().BeOfType<Tandem>();

            vehicleInDatabase.Wheels.ShouldHaveCount(2);
            vehicleInDatabase.Wheels.ElementAt(0).Should().BeOfType<CarbonWheel>();
            vehicleInDatabase.Wheels.ElementAt(0).Id.Should().Be(existingTandem.Wheels.ElementAt(0).Id);
            vehicleInDatabase.Wheels.ElementAt(1).Should().BeOfType<ChromeWheel>();
            vehicleInDatabase.Wheels.ElementAt(1).Id.Should().Be(existingChromeWheel.Id);
        });

        tandemStore.AssertLeftType<Tandem>();
        tandemStore.AssertRightTypes(typeof(ChromeWheel));
    }

    [Fact]
    public async Task Can_add_concrete_base_resources_stored_as_derived_at_ToMany_relationship_endpoint()
    {
        // Arrange
        var manufacturerStore = _testContext.Factory.Services.GetRequiredService<ResourceTypeCaptureStore<VehicleManufacturer, long>>();

        VehicleManufacturer existingManufacturer = _fakers.VehicleManufacturer.Generate();
        existingManufacturer.Vehicles = _fakers.Car.Generate(1).Cast<Vehicle>().ToHashSet();
        ((Car)existingManufacturer.Vehicles.ElementAt(0)).Engine = _fakers.GasolineEngine.Generate();

        Tandem existingTandem = _fakers.Tandem.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Set<VehicleManufacturer>().Add(existingManufacturer);
            dbContext.Vehicles.Add(existingTandem);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "bikes",
                    id = existingTandem.StringId
                }
            }
        };

        string route = $"vehicleManufacturers/{existingManufacturer.StringId}/relationships/vehicles";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            VehicleManufacturer manufacturerInDatabase = await dbContext.Set<VehicleManufacturer>().Include(manufacturer => manufacturer.Vehicles)
                .FirstWithIdAsync(existingManufacturer.Id);

            manufacturerInDatabase.Vehicles.ShouldHaveCount(2);
            manufacturerInDatabase.Vehicles.ElementAt(0).Should().BeOfType<Car>();
            manufacturerInDatabase.Vehicles.ElementAt(0).Id.Should().Be(existingManufacturer.Vehicles.ElementAt(0).Id);
            manufacturerInDatabase.Vehicles.ElementAt(1).Should().BeOfType<Tandem>();
            manufacturerInDatabase.Vehicles.ElementAt(1).Id.Should().Be(existingTandem.Id);
        });

        manufacturerStore.AssertLeftType<VehicleManufacturer>();
        manufacturerStore.AssertRightTypes(typeof(Tandem));
    }

    [Fact]
    public async Task Cannot_add_concrete_derived_resources_stored_as_sibling_derived_at_abstract_ToMany_relationship_endpoint()
    {
        // Arrange
        Bike existingBike = _fakers.Bike.Generate();

        ChromeWheel existingChromeWheel = _fakers.ChromeWheel.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(existingBike);
            dbContext.Wheels.Add(existingChromeWheel);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "carbonWheels",
                    id = existingChromeWheel.StringId
                }
            }
        };

        string route = $"/vehicles/{existingBike.StringId}/relationships/wheels";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("A related resource does not exist.");
        error.Detail.Should().Be($"Related resource of type 'carbonWheels' with ID '{existingChromeWheel.StringId}' in relationship 'wheels' does not exist.");
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_add_unknown_resources_at_ToMany_relationship_endpoint()
    {
        // Arrange
        VehicleManufacturer existingManufacturer = _fakers.VehicleManufacturer.Generate();

        string unknownTruckId = Unknown.StringId.For<Truck, long>();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.VehicleManufacturers.Add(existingManufacturer);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "trucks",
                    id = unknownTruckId
                }
            }
        };

        string route = $"/vehicleManufacturers/{existingManufacturer.StringId}/relationships/vehicles";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("A related resource does not exist.");
        error.Detail.Should().Be($"Related resource of type 'trucks' with ID '{unknownTruckId}' in relationship 'vehicles' does not exist.");
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_add_to_ToMany_relationship_for_left_type_stored_as_sibling_type()
    {
        Truck existingTruck = _fakers.Truck.Generate();
        existingTruck.Engine = _fakers.DieselEngine.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(existingTruck);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = Array.Empty<object>()
        };

        string route = $"/cars/{existingTruck.StringId}/relationships/wheels";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'cars' with ID '{existingTruck.StringId}' does not exist.");
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Can_remove_abstract_resources_at_abstract_ToMany_relationship_endpoint()
    {
        // Arrange
        var tandemStore = _testContext.Factory.Services.GetRequiredService<ResourceTypeCaptureStore<Tandem, long>>();

        Tandem existingTandem = _fakers.Tandem.Generate();
        existingTandem.Wheels = _fakers.ChromeWheel.Generate(2).Cast<Wheel>().ToHashSet();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(existingTandem);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "wheels",
                    id = existingTandem.Wheels.ElementAt(0).StringId
                }
            }
        };

        string route = $"/vehicles/{existingTandem.StringId}/relationships/wheels";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Vehicle vehicleInDatabase = await dbContext.Vehicles.Include(vehicle => vehicle.Wheels).FirstWithIdAsync(existingTandem.Id);

            vehicleInDatabase.Should().BeOfType<Tandem>();

            vehicleInDatabase.Wheels.ShouldHaveCount(1);
            vehicleInDatabase.Wheels.ElementAt(0).Should().BeOfType<ChromeWheel>();
            vehicleInDatabase.Wheels.ElementAt(0).Id.Should().Be(existingTandem.Wheels.ElementAt(1).Id);
        });

        tandemStore.AssertLeftType<Tandem>();
        tandemStore.AssertRightTypes(typeof(ChromeWheel));
    }

    [Fact]
    public async Task Can_remove_concrete_derived_resources_at_abstract_ToMany_relationship_endpoint()
    {
        // Arrange
        var tandemStore = _testContext.Factory.Services.GetRequiredService<ResourceTypeCaptureStore<Tandem, long>>();

        Tandem existingTandem = _fakers.Tandem.Generate();
        existingTandem.Wheels = _fakers.ChromeWheel.Generate(2).Cast<Wheel>().ToHashSet();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(existingTandem);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "chromeWheels",
                    id = existingTandem.Wheels.ElementAt(0).StringId
                }
            }
        };

        string route = $"/vehicles/{existingTandem.StringId}/relationships/wheels";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Vehicle vehicleInDatabase = await dbContext.Vehicles.Include(vehicle => vehicle.Wheels).FirstWithIdAsync(existingTandem.Id);

            vehicleInDatabase.Should().BeOfType<Tandem>();

            vehicleInDatabase.Wheels.ShouldHaveCount(1);
            vehicleInDatabase.Wheels.ElementAt(0).Should().BeOfType<ChromeWheel>();
            vehicleInDatabase.Wheels.ElementAt(0).Id.Should().Be(existingTandem.Wheels.ElementAt(1).Id);
        });

        tandemStore.AssertLeftType<Tandem>();
        tandemStore.AssertRightTypes(typeof(ChromeWheel));
    }

    [Fact]
    public async Task Can_remove_concrete_base_resources_stored_as_derived_at_ToMany_relationship_endpoint()
    {
        // Arrange
        var manufacturerStore = _testContext.Factory.Services.GetRequiredService<ResourceTypeCaptureStore<VehicleManufacturer, long>>();

        VehicleManufacturer existingManufacturer = _fakers.VehicleManufacturer.Generate();
        existingManufacturer.Vehicles = _fakers.Tandem.Generate(1).Cast<Vehicle>().Concat(_fakers.Car.Generate(1)).ToHashSet();
        ((Car)existingManufacturer.Vehicles.ElementAt(1)).Engine = _fakers.GasolineEngine.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Set<VehicleManufacturer>().Add(existingManufacturer);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "bikes",
                    id = existingManufacturer.Vehicles.ElementAt(0).StringId
                }
            }
        };

        string route = $"vehicleManufacturers/{existingManufacturer.StringId}/relationships/vehicles";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            VehicleManufacturer manufacturerInDatabase = await dbContext.Set<VehicleManufacturer>().Include(manufacturer => manufacturer.Vehicles)
                .FirstWithIdAsync(existingManufacturer.Id);

            manufacturerInDatabase.Vehicles.ShouldHaveCount(1);
            manufacturerInDatabase.Vehicles.ElementAt(0).Should().BeOfType<Car>();
            manufacturerInDatabase.Vehicles.ElementAt(0).Id.Should().Be(existingManufacturer.Vehicles.ElementAt(1).Id);
        });

        manufacturerStore.AssertLeftType<VehicleManufacturer>();
        manufacturerStore.AssertRightTypes(typeof(Tandem));
    }

    [Fact]
    public async Task Cannot_remove_concrete_derived_resources_stored_as_sibling_derived_at_abstract_ToMany_relationship_endpoint()
    {
        // Arrange
        Bike existingBike = _fakers.Bike.Generate();
        existingBike.Wheels = _fakers.ChromeWheel.Generate(2).Cast<Wheel>().ToHashSet();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(existingBike);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "carbonWheels",
                    id = existingBike.Wheels.ElementAt(0).StringId
                }
            }
        };

        string route = $"/vehicles/{existingBike.StringId}/relationships/wheels";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(1);

        string? chromeWheelId = existingBike.Wheels.ElementAt(0).StringId;

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("A related resource does not exist.");
        error.Detail.Should().Be($"Related resource of type 'carbonWheels' with ID '{chromeWheelId}' in relationship 'wheels' does not exist.");
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_remove_unknown_resources_at_ToMany_relationship_endpoint()
    {
        // Arrange
        VehicleManufacturer existingManufacturer = _fakers.VehicleManufacturer.Generate();

        string unknownTruckId = Unknown.StringId.For<Truck, long>();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.VehicleManufacturers.Add(existingManufacturer);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "trucks",
                    id = unknownTruckId
                }
            }
        };

        string route = $"/vehicleManufacturers/{existingManufacturer.StringId}/relationships/vehicles";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("A related resource does not exist.");
        error.Detail.Should().Be($"Related resource of type 'trucks' with ID '{unknownTruckId}' in relationship 'vehicles' does not exist.");
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_remove_from_ToMany_relationship_for_left_type_stored_as_sibling_type()
    {
        Truck existingTruck = _fakers.Truck.Generate();
        existingTruck.Engine = _fakers.DieselEngine.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(existingTruck);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = Array.Empty<object>()
        };

        string route = $"/cars/{existingTruck.StringId}/relationships/wheels";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'cars' with ID '{existingTruck.StringId}' does not exist.");
        error.Source.Should().BeNull();
    }
}
