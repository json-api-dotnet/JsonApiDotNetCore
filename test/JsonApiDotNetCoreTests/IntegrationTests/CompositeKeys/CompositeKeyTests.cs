using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompositeKeys;

public sealed class CompositeKeyTests : IClassFixture<IntegrationTestContext<TestableStartup<CompositeDbContext>, CompositeDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<CompositeDbContext>, CompositeDbContext> _testContext;
    private readonly CompositeKeyFakers _fakers = new();

    public CompositeKeyTests(IntegrationTestContext<TestableStartup<CompositeDbContext>, CompositeDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<DealershipsController>();
        testContext.UseController<EnginesController>();
        testContext.UseController<CarsController>();

        testContext.ConfigureServices(services =>
        {
            services.AddResourceRepository<CarCompositeKeyAwareRepository<Car, string?>>();
            services.AddResourceRepository<CarCompositeKeyAwareRepository<Dealership, int>>();
        });
    }

    [Fact]
    public async Task Can_filter_on_ID_in_primary_resources()
    {
        // Arrange
        Car car = _fakers.Car.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Car>();
            dbContext.Cars.Add(car);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/cars?filter=any(id,'{car.RegionId}:{car.LicensePlate}','999:XX-YY-22')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(car.StringId);
    }

    [Fact]
    public async Task Can_get_primary_resource_by_ID()
    {
        // Arrange
        Car car = _fakers.Car.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Car>();
            dbContext.Cars.Add(car);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/cars/{car.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(car.StringId);
    }

    [Fact]
    public async Task Can_sort_on_ID()
    {
        // Arrange
        Car car = _fakers.Car.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Car>();
            dbContext.Cars.Add(car);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/cars?sort=id";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(car.StringId);
    }

    [Fact]
    public async Task Can_select_ID()
    {
        // Arrange
        Car car = _fakers.Car.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Car>();
            dbContext.Cars.Add(car);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/cars?fields[cars]=id";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(car.StringId);
    }

    [Fact]
    public async Task Can_create_resource()
    {
        // Arrange
        Engine existingEngine = _fakers.Engine.GenerateOne();

        Car newCar = _fakers.Car.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Car>();
            dbContext.Engines.Add(existingEngine);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "cars",
                attributes = new
                {
                    regionId = newCar.RegionId,
                    licensePlate = newCar.LicensePlate
                },
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

        const string route = "/cars";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Car? carInDatabase = await dbContext.Cars.FirstOrDefaultAsync(car => car.RegionId == newCar.RegionId && car.LicensePlate == newCar.LicensePlate);

            carInDatabase.Should().NotBeNull();
            carInDatabase.Id.Should().Be($"{newCar.RegionId}:{newCar.LicensePlate}");
        });
    }

    [Fact]
    public async Task Can_create_OneToOne_relationship()
    {
        // Arrange
        Car existingCar = _fakers.Car.GenerateOne();
        Engine existingEngine = _fakers.Engine.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Car>();
            dbContext.AddInRange(existingCar, existingEngine);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "engines",
                id = existingEngine.StringId,
                relationships = new
                {
                    car = new
                    {
                        data = new
                        {
                            type = "cars",
                            id = existingCar.StringId
                        }
                    }
                }
            }
        };

        string route = $"/engines/{existingEngine.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Engine engineInDatabase = await dbContext.Engines.Include(engine => engine.Car).FirstWithIdAsync(existingEngine.Id);

            engineInDatabase.Car.Should().NotBeNull();
            engineInDatabase.Car.Id.Should().Be(existingCar.StringId);
        });
    }

    [Fact]
    public async Task Can_clear_OneToOne_relationship()
    {
        // Arrange
        Engine existingEngine = _fakers.Engine.GenerateOne();
        existingEngine.Car = _fakers.Car.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Car>();
            dbContext.Engines.Add(existingEngine);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "engines",
                id = existingEngine.StringId,
                relationships = new
                {
                    car = new
                    {
                        data = (object?)null
                    }
                }
            }
        };

        string route = $"/engines/{existingEngine.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Engine engineInDatabase = await dbContext.Engines.Include(engine => engine.Car).FirstWithIdAsync(existingEngine.Id);

            engineInDatabase.Car.Should().BeNull();
        });
    }

    [Fact]
    public async Task Can_remove_from_OneToMany_relationship()
    {
        // Arrange
        Dealership existingDealership = _fakers.Dealership.GenerateOne();
        existingDealership.Inventory = _fakers.Car.GenerateSet(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Car>();
            dbContext.Dealerships.Add(existingDealership);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "cars",
                    id = existingDealership.Inventory.ElementAt(0).StringId
                }
            }
        };

        string route = $"/dealerships/{existingDealership.StringId}/relationships/inventory";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Dealership dealershipInDatabase = await dbContext.Dealerships.Include(dealership => dealership.Inventory).FirstWithIdAsync(existingDealership.Id);

            dealershipInDatabase.Inventory.Should().HaveCount(1);
            dealershipInDatabase.Inventory.Should().ContainSingle(car => car.Id == existingDealership.Inventory.ElementAt(1).Id);
        });
    }

    [Fact]
    public async Task Can_add_to_OneToMany_relationship()
    {
        // Arrange
        Dealership existingDealership = _fakers.Dealership.GenerateOne();
        Car existingCar = _fakers.Car.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Car>();
            dbContext.AddInRange(existingDealership, existingCar);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "cars",
                    id = existingCar.StringId
                }
            }
        };

        string route = $"/dealerships/{existingDealership.StringId}/relationships/inventory";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Dealership dealershipInDatabase = await dbContext.Dealerships.Include(dealership => dealership.Inventory).FirstWithIdAsync(existingDealership.Id);

            dealershipInDatabase.Inventory.Should().HaveCount(1);
            dealershipInDatabase.Inventory.Should().ContainSingle(car => car.Id == existingCar.Id);
        });
    }

    [Fact]
    public async Task Can_replace_OneToMany_relationship()
    {
        // Arrange
        Dealership existingDealership = _fakers.Dealership.GenerateOne();
        existingDealership.Inventory = _fakers.Car.GenerateSet(2);

        Car existingCar = _fakers.Car.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Car>();
            dbContext.AddInRange(existingDealership, existingCar);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "cars",
                    id = existingDealership.Inventory.ElementAt(0).StringId
                },
                new
                {
                    type = "cars",
                    id = existingCar.StringId
                }
            }
        };

        string route = $"/dealerships/{existingDealership.StringId}/relationships/inventory";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Dealership dealershipInDatabase = await dbContext.Dealerships.Include(dealership => dealership.Inventory).FirstWithIdAsync(existingDealership.Id);

            dealershipInDatabase.Inventory.Should().HaveCount(2);
            dealershipInDatabase.Inventory.Should().ContainSingle(car => car.Id == existingCar.Id);
            dealershipInDatabase.Inventory.Should().ContainSingle(car => car.Id == existingDealership.Inventory.ElementAt(0).Id);
        });
    }

    [Fact]
    public async Task Cannot_remove_from_ManyToOne_relationship_for_unknown_relationship_ID()
    {
        // Arrange
        Dealership existingDealership = _fakers.Dealership.GenerateOne();

        string unknownCarId = _fakers.Car.GenerateOne().StringId!;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Car>();
            dbContext.Dealerships.Add(existingDealership);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "cars",
                    id = unknownCarId
                }
            }
        };

        string route = $"/dealerships/{existingDealership.StringId}/relationships/inventory";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("A related resource does not exist.");
        error.Detail.Should().Be($"Related resource of type 'cars' with ID '{unknownCarId}' in relationship 'inventory' does not exist.");
    }

    [Fact]
    public async Task Can_delete_resource()
    {
        // Arrange
        Car existingCar = _fakers.Car.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Car>();
            dbContext.Cars.Add(existingCar);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/cars/{existingCar.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Car? carInDatabase =
                await dbContext.Cars.FirstOrDefaultAsync(car => car.RegionId == existingCar.RegionId && car.LicensePlate == existingCar.LicensePlate);

            carInDatabase.Should().BeNull();
        });
    }

    [Fact]
    public async Task Can_remove_from_ManyToMany_relationship()
    {
        // Arrange
        Dealership existingDealership = _fakers.Dealership.GenerateOne();
        existingDealership.SoldCars = _fakers.Car.GenerateSet(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Car>();
            dbContext.Dealerships.Add(existingDealership);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "cars",
                    id = existingDealership.SoldCars.ElementAt(1).StringId
                }
            }
        };

        string route = $"/dealerships/{existingDealership.StringId}/relationships/soldCars";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Dealership dealershipInDatabase = await dbContext.Dealerships.Include(dealership => dealership.SoldCars).FirstWithIdAsync(existingDealership.Id);

            dealershipInDatabase.SoldCars.Should().HaveCount(1);
            dealershipInDatabase.SoldCars.Single().Id.Should().Be(existingDealership.SoldCars.ElementAt(0).Id);

            List<Car> carsInDatabase = await dbContext.Cars.ToListAsync();
            carsInDatabase.Should().HaveCount(2);
        });
    }
}
