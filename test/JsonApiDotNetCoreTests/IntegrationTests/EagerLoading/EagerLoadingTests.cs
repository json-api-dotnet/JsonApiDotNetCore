using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.EagerLoading;

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

        testContext.ConfigureServices(services =>
        {
            services.AddResourceDefinition<BuildingDefinition>();
            services.AddResourceRepository<BuildingRepository>();
        });
    }

    [Fact]
    public async Task Can_get_primary_resource_with_eager_loads()
    {
        // Arrange
        Building building = _fakers.Building.GenerateOne();
        building.Windows = _fakers.Window.GenerateList(4);
        building.PrimaryDoor = _fakers.Door.GenerateOne();
        building.SecondaryDoor = _fakers.Door.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Buildings.Add(building);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/buildings/{building.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(building.StringId);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("number").WhoseValue.Should().Be(building.Number);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("windowCount").WhoseValue.Should().Be(4);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("primaryDoorColor").WhoseValue.Should().Be(building.PrimaryDoor.Color);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("secondaryDoorColor").WhoseValue.Should().Be(building.SecondaryDoor.Color);
    }

    [Fact]
    public async Task Can_get_primary_resource_with_nested_eager_loads()
    {
        // Arrange
        Street street = _fakers.Street.GenerateOne();
        street.Buildings = _fakers.Building.GenerateList(2);

        street.Buildings[0].Windows = _fakers.Window.GenerateList(2);
        street.Buildings[0].PrimaryDoor = _fakers.Door.GenerateOne();

        street.Buildings[1].Windows = _fakers.Window.GenerateList(3);
        street.Buildings[1].PrimaryDoor = _fakers.Door.GenerateOne();
        street.Buildings[1].SecondaryDoor = _fakers.Door.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Streets.Add(street);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/streets/{street.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(street.StringId);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("name").WhoseValue.Should().Be(street.Name);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("buildingCount").WhoseValue.Should().Be(2);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("doorTotalCount").WhoseValue.Should().Be(3);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("windowTotalCount").WhoseValue.Should().Be(5);
    }

    [Fact]
    public async Task Can_get_primary_resource_with_fieldset()
    {
        // Arrange
        Street street = _fakers.Street.GenerateOne();
        street.Buildings = _fakers.Building.GenerateList(1);
        street.Buildings[0].Windows = _fakers.Window.GenerateList(3);
        street.Buildings[0].PrimaryDoor = _fakers.Door.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Streets.Add(street);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/streets/{street.StringId}?fields[streets]=windowTotalCount";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(street.StringId);
        responseDocument.Data.SingleValue.Attributes.Should().HaveCount(1);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("windowTotalCount").WhoseValue.Should().Be(3);
        responseDocument.Data.SingleValue.Relationships.Should().BeNull();
    }

    [Fact]
    public async Task Can_get_primary_resource_with_includes()
    {
        // Arrange
        State state = _fakers.State.GenerateOne();
        state.Cities = _fakers.City.GenerateList(1);
        state.Cities[0].Streets = _fakers.Street.GenerateList(1);
        state.Cities[0].Streets[0].Buildings = _fakers.Building.GenerateList(1);
        state.Cities[0].Streets[0].Buildings[0].PrimaryDoor = _fakers.Door.GenerateOne();
        state.Cities[0].Streets[0].Buildings[0].Windows = _fakers.Window.GenerateList(3);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.States.Add(state);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/states/{state.StringId}?include=cities.streets";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(state.StringId);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("name").WhoseValue.Should().Be(state.Name);

        responseDocument.Included.Should().HaveCount(2);

        responseDocument.Included[0].Type.Should().Be("cities");
        responseDocument.Included[0].Id.Should().Be(state.Cities[0].StringId);
        responseDocument.Included[0].Attributes.Should().ContainKey("name").WhoseValue.Should().Be(state.Cities[0].Name);

        responseDocument.Included[1].Type.Should().Be("streets");
        responseDocument.Included[1].Id.Should().Be(state.Cities[0].Streets[0].StringId);
        responseDocument.Included[1].Attributes.Should().ContainKey("buildingCount").WhoseValue.Should().Be(1);
        responseDocument.Included[1].Attributes.Should().ContainKey("doorTotalCount").WhoseValue.Should().Be(1);
        responseDocument.Included[1].Attributes.Should().ContainKey("windowTotalCount").WhoseValue.Should().Be(3);
    }

    [Fact]
    public async Task Can_get_secondary_resources_with_include_and_fieldsets()
    {
        // Arrange
        State state = _fakers.State.GenerateOne();
        state.Cities = _fakers.City.GenerateList(1);
        state.Cities[0].Streets = _fakers.Street.GenerateList(1);
        state.Cities[0].Streets[0].Buildings = _fakers.Building.GenerateList(1);
        state.Cities[0].Streets[0].Buildings[0].PrimaryDoor = _fakers.Door.GenerateOne();
        state.Cities[0].Streets[0].Buildings[0].SecondaryDoor = _fakers.Door.GenerateOne();
        state.Cities[0].Streets[0].Buildings[0].Windows = _fakers.Window.GenerateList(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.States.Add(state);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/states/{state.StringId}/cities?include=streets&fields[cities]=name&fields[streets]=doorTotalCount,windowTotalCount";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(state.Cities[0].StringId);
        responseDocument.Data.ManyValue[0].Attributes.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.Should().ContainKey("name").WhoseValue.Should().Be(state.Cities[0].Name);
        responseDocument.Data.ManyValue[0].Relationships.Should().BeNull();

        responseDocument.Included.Should().HaveCount(1);
        responseDocument.Included[0].Type.Should().Be("streets");
        responseDocument.Included[0].Id.Should().Be(state.Cities[0].Streets[0].StringId);
        responseDocument.Included[0].Attributes.Should().HaveCount(2);
        responseDocument.Included[0].Attributes.Should().ContainKey("doorTotalCount").WhoseValue.Should().Be(2);
        responseDocument.Included[0].Attributes.Should().ContainKey("windowTotalCount").WhoseValue.Should().Be(1);
        responseDocument.Included[0].Relationships.Should().BeNull();
    }

    [Fact]
    public async Task Can_create_resource()
    {
        // Arrange
        Building newBuilding = _fakers.Building.GenerateOne();

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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("number").WhoseValue.Should().Be(newBuilding.Number);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("windowCount").WhoseValue.Should().Be(0);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("primaryDoorColor").WhoseValue.Should().Be("(unspecified)");
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("secondaryDoorColor").WhoseValue.Should().BeNull();

        int newBuildingId = int.Parse(responseDocument.Data.SingleValue.Id.Should().NotBeNull().And.Subject);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:wrap_after_property_in_chained_method_calls true

            Building? buildingInDatabase = await dbContext.Buildings
                .Include(building => building.PrimaryDoor)
                .Include(building => building.SecondaryDoor)
                .Include(building => building.Windows)
                .FirstWithIdOrDefaultAsync(newBuildingId);

            // @formatter:wrap_after_property_in_chained_method_calls restore
            // @formatter:wrap_chained_method_calls restore

            buildingInDatabase.Should().NotBeNull();
            buildingInDatabase.Number.Should().Be(newBuilding.Number);
            buildingInDatabase.PrimaryDoor.Should().NotBeNull();
            buildingInDatabase.PrimaryDoor.Color.Should().Be("(unspecified)");
            buildingInDatabase.SecondaryDoor.Should().BeNull();
            buildingInDatabase.Windows.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task Can_update_resource()
    {
        // Arrange
        Building existingBuilding = _fakers.Building.GenerateOne();
        existingBuilding.PrimaryDoor = _fakers.Door.GenerateOne();
        existingBuilding.SecondaryDoor = _fakers.Door.GenerateOne();
        existingBuilding.Windows = _fakers.Window.GenerateList(2);

        string newBuildingNumber = _fakers.Building.GenerateOne().Number;
        string newPrimaryDoorColor = _fakers.Door.GenerateOne().Color;

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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:wrap_after_property_in_chained_method_calls true

            Building? buildingInDatabase = await dbContext.Buildings
                .Include(building => building.PrimaryDoor)
                .Include(building => building.SecondaryDoor)
                .Include(building => building.Windows)
                .FirstWithIdOrDefaultAsync(existingBuilding.Id);

            // @formatter:wrap_after_property_in_chained_method_calls restore
            // @formatter:wrap_chained_method_calls restore

            buildingInDatabase.Should().NotBeNull();
            buildingInDatabase.Number.Should().Be(newBuildingNumber);
            buildingInDatabase.PrimaryDoor.Should().NotBeNull();
            buildingInDatabase.PrimaryDoor.Color.Should().Be(newPrimaryDoorColor);
            buildingInDatabase.SecondaryDoor.Should().NotBeNull();
            buildingInDatabase.Windows.Should().HaveCount(2);
        });
    }

    [Fact]
    public async Task Cannot_update_resource_when_primaryDoorColor_is_set_to_null()
    {
        // Arrange
        Building existingBuilding = _fakers.Building.GenerateOne();
        existingBuilding.PrimaryDoor = _fakers.Door.GenerateOne();

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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Input validation failed.");
        error.Detail.Should().Be("The PrimaryDoorColor field is required.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/primaryDoorColor");
    }

    [Fact]
    public async Task Can_delete_resource()
    {
        // Arrange
        Building existingBuilding = _fakers.Building.GenerateOne();
        existingBuilding.PrimaryDoor = _fakers.Door.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Buildings.Add(existingBuilding);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/buildings/{existingBuilding.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Building? buildingInDatabase = await dbContext.Buildings.FirstWithIdOrDefaultAsync(existingBuilding.Id);

            buildingInDatabase.Should().BeNull();
        });
    }
}
