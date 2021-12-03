using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.EagerLoading
{
    public sealed class EagerLoadingTests : IClassFixture<IntegrationTestContext<TestableStartup<EagerLoadingDbContext>, EagerLoadingDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<EagerLoadingDbContext>, EagerLoadingDbContext> _testContext;
        private readonly EagerLoadingFakers _fakers = new();

        public EagerLoadingTests(IntegrationTestContext<TestableStartup<EagerLoadingDbContext>, EagerLoadingDbContext> testContext)
        {
            _testContext = testContext;

            testContext.UseController<StreetsController>();
            testContext.UseController<StatesController>();
            testContext.UseController<BuildingsController>();

            testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddResourceDefinition<BuildingDefinition>();
                services.AddResourceRepository<BuildingRepository>();
            });
        }

        [Fact]
        public async Task Can_get_primary_resource_with_eager_loads()
        {
            // Arrange
            Building building = _fakers.Building.Generate();
            building.Windows = _fakers.Window.Generate(4);
            building.PrimaryDoor = _fakers.Door.Generate();
            building.SecondaryDoor = _fakers.Door.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Buildings.Add(building);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/buildings/{building.StringId}";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.SingleValue.ShouldNotBeNull();
            responseDocument.Data.SingleValue.Id.Should().Be(building.StringId);
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("number").With(value => value.Should().Be(building.Number));
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("windowCount").With(value => value.Should().Be(4));
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("primaryDoorColor").With(value => value.Should().Be(building.PrimaryDoor.Color));
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("secondaryDoorColor").With(value => value.Should().Be(building.SecondaryDoor.Color));
        }

        [Fact]
        public async Task Can_get_primary_resource_with_nested_eager_loads()
        {
            // Arrange
            Street street = _fakers.Street.Generate();
            street.Buildings = _fakers.Building.Generate(2);

            street.Buildings[0].Windows = _fakers.Window.Generate(2);
            street.Buildings[0].PrimaryDoor = _fakers.Door.Generate();

            street.Buildings[1].Windows = _fakers.Window.Generate(3);
            street.Buildings[1].PrimaryDoor = _fakers.Door.Generate();
            street.Buildings[1].SecondaryDoor = _fakers.Door.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Streets.Add(street);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/streets/{street.StringId}";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.SingleValue.ShouldNotBeNull();
            responseDocument.Data.SingleValue.Id.Should().Be(street.StringId);
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("name").With(value => value.Should().Be(street.Name));
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("buildingCount").With(value => value.Should().Be(2));
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("doorTotalCount").With(value => value.Should().Be(3));
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("windowTotalCount").With(value => value.Should().Be(5));
        }

        [Fact]
        public async Task Can_get_primary_resource_with_fieldset()
        {
            // Arrange
            Street street = _fakers.Street.Generate();
            street.Buildings = _fakers.Building.Generate(1);
            street.Buildings[0].Windows = _fakers.Window.Generate(3);
            street.Buildings[0].PrimaryDoor = _fakers.Door.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Streets.Add(street);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/streets/{street.StringId}?fields[streets]=windowTotalCount";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.SingleValue.ShouldNotBeNull();
            responseDocument.Data.SingleValue.Id.Should().Be(street.StringId);
            responseDocument.Data.SingleValue.Attributes.ShouldHaveCount(1);
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("windowTotalCount").With(value => value.Should().Be(3));
            responseDocument.Data.SingleValue.Relationships.Should().BeNull();
        }

        [Fact]
        public async Task Can_get_primary_resource_with_includes()
        {
            // Arrange
            State state = _fakers.State.Generate();
            state.Cities = _fakers.City.Generate(1);
            state.Cities[0].Streets = _fakers.Street.Generate(1);
            state.Cities[0].Streets[0].Buildings = _fakers.Building.Generate(1);
            state.Cities[0].Streets[0].Buildings[0].PrimaryDoor = _fakers.Door.Generate();
            state.Cities[0].Streets[0].Buildings[0].Windows = _fakers.Window.Generate(3);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.States.Add(state);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/states/{state.StringId}?include=cities.streets";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.SingleValue.ShouldNotBeNull();
            responseDocument.Data.SingleValue.Id.Should().Be(state.StringId);
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("name").With(value => value.Should().Be(state.Name));

            responseDocument.Included.ShouldHaveCount(2);

            responseDocument.Included[0].Type.Should().Be("cities");
            responseDocument.Included[0].Id.Should().Be(state.Cities[0].StringId);
            responseDocument.Included[0].Attributes.ShouldContainKey("name").With(value => value.Should().Be(state.Cities[0].Name));

            responseDocument.Included[1].Type.Should().Be("streets");
            responseDocument.Included[1].Id.Should().Be(state.Cities[0].Streets[0].StringId);
            responseDocument.Included[1].Attributes.ShouldContainKey("buildingCount").With(value => value.Should().Be(1));
            responseDocument.Included[1].Attributes.ShouldContainKey("doorTotalCount").With(value => value.Should().Be(1));
            responseDocument.Included[1].Attributes.ShouldContainKey("windowTotalCount").With(value => value.Should().Be(3));
        }

        [Fact]
        public async Task Can_get_secondary_resources_with_include_and_fieldsets()
        {
            // Arrange
            State state = _fakers.State.Generate();
            state.Cities = _fakers.City.Generate(1);
            state.Cities[0].Streets = _fakers.Street.Generate(1);
            state.Cities[0].Streets[0].Buildings = _fakers.Building.Generate(1);
            state.Cities[0].Streets[0].Buildings[0].PrimaryDoor = _fakers.Door.Generate();
            state.Cities[0].Streets[0].Buildings[0].SecondaryDoor = _fakers.Door.Generate();
            state.Cities[0].Streets[0].Buildings[0].Windows = _fakers.Window.Generate(1);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.States.Add(state);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/states/{state.StringId}/cities?include=streets&fields[cities]=name&fields[streets]=doorTotalCount,windowTotalCount";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.ManyValue.ShouldHaveCount(1);
            responseDocument.Data.ManyValue[0].Id.Should().Be(state.Cities[0].StringId);
            responseDocument.Data.ManyValue[0].Attributes.ShouldHaveCount(1);
            responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("name").With(value => value.Should().Be(state.Cities[0].Name));
            responseDocument.Data.ManyValue[0].Relationships.Should().BeNull();

            responseDocument.Included.ShouldHaveCount(1);
            responseDocument.Included[0].Type.Should().Be("streets");
            responseDocument.Included[0].Id.Should().Be(state.Cities[0].Streets[0].StringId);
            responseDocument.Included[0].Attributes.ShouldHaveCount(2);
            responseDocument.Included[0].Attributes.ShouldContainKey("doorTotalCount").With(value => value.Should().Be(2));
            responseDocument.Included[0].Attributes.ShouldContainKey("windowTotalCount").With(value => value.Should().Be(1));
            responseDocument.Included[0].Relationships.Should().BeNull();
        }

        [Fact]
        public async Task Can_create_resource()
        {
            // Arrange
            Building newBuilding = _fakers.Building.Generate();

            var requestBody = new
            {
                data = new
                {
                    type = "buildings",
                    attributes = new
                    {
                        number = newBuilding.Number
                    }
                }
            };

            const string route = "/buildings";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.Data.SingleValue.ShouldNotBeNull();
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("number").With(value => value.Should().Be(newBuilding.Number));
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("windowCount").With(value => value.Should().Be(0));
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("primaryDoorColor").With(value => value.Should().Be("(unspecified)"));
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("secondaryDoorColor").With(value => value.Should().BeNull());

            int newBuildingId = int.Parse(responseDocument.Data.SingleValue.Id.ShouldNotBeNull());

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                // @formatter:wrap_chained_method_calls chop_always
                // @formatter:keep_existing_linebreaks true

                Building? buildingInDatabase = await dbContext.Buildings
                    .Include(building => building.PrimaryDoor)
                    .Include(building => building.SecondaryDoor)
                    .Include(building => building.Windows)
                    .FirstWithIdOrDefaultAsync(newBuildingId);

                // @formatter:keep_existing_linebreaks restore
                // @formatter:wrap_chained_method_calls restore

                buildingInDatabase.ShouldNotBeNull();
                buildingInDatabase.Number.Should().Be(newBuilding.Number);
                buildingInDatabase.PrimaryDoor.ShouldNotBeNull();
                buildingInDatabase.PrimaryDoor.Color.Should().Be("(unspecified)");
                buildingInDatabase.SecondaryDoor.Should().BeNull();
                buildingInDatabase.Windows.Should().BeEmpty();
            });
        }

        [Fact]
        public async Task Can_update_resource()
        {
            // Arrange
            Building existingBuilding = _fakers.Building.Generate();
            existingBuilding.PrimaryDoor = _fakers.Door.Generate();
            existingBuilding.SecondaryDoor = _fakers.Door.Generate();
            existingBuilding.Windows = _fakers.Window.Generate(2);

            string newBuildingNumber = _fakers.Building.Generate().Number;
            string newPrimaryDoorColor = _fakers.Door.Generate().Color;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Buildings.Add(existingBuilding);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "buildings",
                    id = existingBuilding.StringId,
                    attributes = new
                    {
                        number = newBuildingNumber,
                        primaryDoorColor = newPrimaryDoorColor
                    }
                }
            };

            string route = $"/buildings/{existingBuilding.StringId}";

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                // @formatter:wrap_chained_method_calls chop_always
                // @formatter:keep_existing_linebreaks true

                Building? buildingInDatabase = await dbContext.Buildings
                    .Include(building => building.PrimaryDoor)
                    .Include(building => building.SecondaryDoor)
                    .Include(building => building.Windows)
                    .FirstWithIdOrDefaultAsync(existingBuilding.Id);

                // @formatter:keep_existing_linebreaks restore
                // @formatter:wrap_chained_method_calls restore

                buildingInDatabase.ShouldNotBeNull();
                buildingInDatabase.Number.Should().Be(newBuildingNumber);
                buildingInDatabase.PrimaryDoor.ShouldNotBeNull();
                buildingInDatabase.PrimaryDoor.Color.Should().Be(newPrimaryDoorColor);
                buildingInDatabase.SecondaryDoor.ShouldNotBeNull();
                buildingInDatabase.Windows.ShouldHaveCount(2);
            });
        }

        [Fact]
        public async Task Cannot_update_resource_when_primaryDoorColor_is_set_to_null()
        {
            // Arrange
            Building existingBuilding = _fakers.Building.Generate();
            existingBuilding.PrimaryDoor = _fakers.Door.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Buildings.Add(existingBuilding);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "buildings",
                    id = existingBuilding.StringId,
                    attributes = new
                    {
                        primaryDoorColor = (string?)null
                    }
                }
            };

            string route = $"/buildings/{existingBuilding.StringId}";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.ShouldHaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error.Title.Should().Be("Input validation failed.");
            error.Detail.Should().Be("The PrimaryDoorColor field is required.");
            error.Source.ShouldNotBeNull();
            error.Source.Pointer.Should().Be("/data/attributes/primaryDoorColor");
        }

        [Fact]
        public async Task Can_delete_resource()
        {
            // Arrange
            Building existingBuilding = _fakers.Building.Generate();
            existingBuilding.PrimaryDoor = _fakers.Door.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Buildings.Add(existingBuilding);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/buildings/{existingBuilding.StringId}";

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                Building? buildingInDatabase = await dbContext.Buildings.FirstWithIdOrDefaultAsync(existingBuilding.Id);

                buildingInDatabase.Should().BeNull();
            });
        }
    }
}
