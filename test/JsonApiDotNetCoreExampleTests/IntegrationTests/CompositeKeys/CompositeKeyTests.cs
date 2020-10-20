using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.CompositeKeys
{
    public sealed class CompositeKeyTests : IClassFixture<IntegrationTestContext<TestableStartup<CompositeDbContext>, CompositeDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<CompositeDbContext>, CompositeDbContext> _testContext;

        public CompositeKeyTests(IntegrationTestContext<TestableStartup<CompositeDbContext>, CompositeDbContext> testContext)
        {
            _testContext = testContext;

            testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddScoped<IResourceRepository<Car, string>, CarRepository>();
            });
            
            var options = (JsonApiOptions) testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.AllowClientGeneratedIds = true;
        }

        [Fact]
        public async Task Can_filter_on_composite_key_in_primary_resources()
        {
            // Arrange
            var car = new Car
            {
                RegionId = 123,
                LicensePlate = "AA-BB-11"
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Car>();
                dbContext.Cars.Add(car);
                await dbContext.SaveChangesAsync();
            });

            var route = "/cars?filter=any(id,'123:AA-BB-11','999:XX-YY-22')";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(car.StringId);
        }

        [Fact]
        public async Task Can_get_primary_resource_by_ID()
        {
            // Arrange
            var car = new Car
            {
                RegionId = 123,
                LicensePlate = "AA-BB-11"
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Car>();
                dbContext.Cars.Add(car);
                await dbContext.SaveChangesAsync();
            });

            var route = "/cars/" + car.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(car.StringId);
        }

        [Fact]
        public async Task Can_sort_on_composite_primary_key()
        {
            // Arrange
            var car = new Car
            {
                RegionId = 123,
                LicensePlate = "AA-BB-11"
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Car>();
                dbContext.Cars.Add(car);
                await dbContext.SaveChangesAsync();
            });

            var route = "/cars?sort=id";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(car.StringId);
        }

        [Fact]
        public async Task Can_select_composite_primary_key()
        {
            // Arrange
            var car = new Car
            {
                RegionId = 123,
                LicensePlate = "AA-BB-11"
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Car>();
                dbContext.Cars.Add(car);
                await dbContext.SaveChangesAsync();
            });

            var route = "/cars?fields=id";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(car.StringId);
        }

        [Fact]
        public async Task Can_create_resource_with_composite_primary_key()
        {
            // Arrange
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Car>();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "cars",
                    attributes = new
                    {
                        regionId = 123,
                        licensePlate = "AA-BB-11"
                    }
                }
            };

            var route = "/cars";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
        }

        [Fact]
        public async Task Can_remove_relationship_from_resource_with_composite_foreign_key()
        {
            // Arrange
            var engine = new Engine
            {
                SerialCode = "1234567890",
                Car = new Car
                {
                    RegionId = 123,
                    LicensePlate = "AA-BB-11"
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Car>();
                dbContext.Engines.Add(engine);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "engines",
                    id = engine.StringId,
                    relationships = new Dictionary<string, object>
                    {
                        ["car"] = new
                        {
                            data = (object)null
                        }
                    }
                }
            };

            var route = "/engines/" + engine.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);
            
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var engineInDatabase = await dbContext.Engines
                    .Include(e => e.Car)
                    .SingleAsync(e => e.Id == engine.Id);

                engineInDatabase.Car.Should().BeNull();
            });
        }

        [Fact]
        public async Task Can_assign_relationship_to_resource_with_composite_foreign_key()
        {
            // Arrange
            var car = new Car
            {
                RegionId = 123,
                LicensePlate = "AA-BB-11"
            };

            var engine = new Engine
            {
                SerialCode = "1234567890"
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Car>();
                dbContext.AddRange(car, engine);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "engines",
                    id = engine.StringId,
                    relationships = new Dictionary<string, object>
                    {
                        ["car"] = new
                        {
                            data = new
                            {
                                type = "cars",
                                id = car.StringId
                            }
                        }
                    }
                }
            };

            var route = "/engines/" + engine.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var engineInDatabase = await dbContext.Engines
                    .Include(e => e.Car)
                    .SingleAsync(e => e.Id == engine.Id);

                engineInDatabase.Car.Should().NotBeNull();
                engineInDatabase.Car.Id.Should().Be(car.StringId);
            });
        }

        [Fact]
        public async Task Can_delete_resource_with_composite_primary_key()
        {
            // Arrange
            var car = new Car
            {
                RegionId = 123,
                LicensePlate = "AA-BB-11"
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Car>();
                dbContext.Cars.Add(car);
                await dbContext.SaveChangesAsync();
            });

            var route = "/cars/" + car.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();
        }
    }
}
