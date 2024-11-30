using FluentAssertions;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.OpenApi.Client.NSwag;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenApiNSwagEndToEndTests.OnlyRelationshipsInheritance.GeneratedCode;
using OpenApiTests;
using OpenApiTests.ResourceInheritance;
using OpenApiTests.ResourceInheritance.Models;
using OpenApiTests.ResourceInheritance.OnlyRelationships;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiNSwagEndToEndTests.ResourceInheritance.OnlyRelationships;

public sealed class OnlyRelationshipsInheritanceTests
    : IClassFixture<IntegrationTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext>>, IDisposable
{
    private readonly IntegrationTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> _testContext;
    private readonly XUnitLogHttpMessageHandler _logHttpMessageHandler;
    private readonly ResourceInheritanceFakers _fakers = new();

    public OnlyRelationshipsInheritanceTests(IntegrationTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> testContext,
        ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _logHttpMessageHandler = new XUnitLogHttpMessageHandler(testOutputHelper);

        testContext.UseInheritanceControllers(true);

        testContext.ConfigureServices(services =>
        {
            services.AddSingleton<IJsonApiEndpointFilter, OnlyRelationshipsEndpointFilter>();
            services.AddSingleton<IAtomicOperationFilter, OnlyRelationshipsOperationFilter>();
        });
    }

    [Fact]
    public async Task Can_get_ToOne_relationship_at_abstract_base_endpoint()
    {
        // Arrange
        Bedroom bedroom = _fakers.Bedroom.GenerateOne();
        bedroom.Residence = _fakers.FamilyHome.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Rooms.Add(bedroom);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new OnlyRelationshipsInheritanceClient(httpClient);

        // Act
        ResidenceIdentifierResponseDocument? response =
            await ApiResponse.TranslateAsync(async () => await apiClient.GetRoomResidenceRelationshipAsync(bedroom.StringId!));

        // Assert
        response.ShouldNotBeNull();
        response.Data.Should().BeOfType<FamilyHomeIdentifierInResponse>();
        response.Data.Id.Should().Be(bedroom.Residence.StringId);
    }

    [Fact]
    public async Task Can_get_ToOne_relationship_at_concrete_derived_endpoint()
    {
        // Arrange
        Bedroom bedroom = _fakers.Bedroom.GenerateOne();
        bedroom.Residence = _fakers.FamilyHome.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Rooms.Add(bedroom);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new OnlyRelationshipsInheritanceClient(httpClient);

        // Act
        ResidenceIdentifierResponseDocument? response =
            await ApiResponse.TranslateAsync(async () => await apiClient.GetBedroomResidenceRelationshipAsync(bedroom.StringId!));

        // Assert
        response.ShouldNotBeNull();
        response.Data.Should().BeOfType<FamilyHomeIdentifierInResponse>();
        response.Data.Id.Should().Be(bedroom.Residence.StringId);
    }

    [Fact]
    public async Task Can_get_ToMany_relationship_at_concrete_base_endpoint()
    {
        // Arrange
        Kitchen kitchen = _fakers.Kitchen.GenerateOne();
        Bedroom bedroom = _fakers.Bedroom.GenerateOne();

        FamilyHome familyHome = _fakers.FamilyHome.GenerateOne();
        familyHome.Rooms.Add(kitchen);
        familyHome.Rooms.Add(bedroom);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.FamilyHomes.Add(familyHome);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new OnlyRelationshipsInheritanceClient(httpClient);

        // Act
        RoomIdentifierCollectionResponseDocument? response =
            await ApiResponse.TranslateAsync(async () => await apiClient.GetResidenceRoomsRelationshipAsync(familyHome.StringId!));

        // Assert
        response.ShouldNotBeNull();
        response.Data.ShouldHaveCount(2);

        response.Data.OfType<KitchenIdentifierInResponse>().Should().ContainSingle(kitchenIdentifier => kitchenIdentifier.Id == kitchen.StringId);
        response.Data.OfType<BedroomIdentifierInResponse>().Should().ContainSingle(bedroomIdentifier => bedroomIdentifier.Id == bedroom.StringId);
    }

    [Fact]
    public async Task Can_get_ToMany_relationship_at_concrete_derived_endpoint()
    {
        // Arrange
        Bathroom bathroom = _fakers.Bathroom.GenerateOne();
        Toilet toilet = _fakers.Toilet.GenerateOne();

        Mansion mansion = _fakers.Mansion.GenerateOne();
        mansion.Rooms.Add(bathroom);
        mansion.Rooms.Add(toilet);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Mansions.Add(mansion);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new OnlyRelationshipsInheritanceClient(httpClient);

        // Act
        RoomIdentifierCollectionResponseDocument? response =
            await ApiResponse.TranslateAsync(async () => await apiClient.GetMansionRoomsRelationshipAsync(mansion.StringId!));

        // Assert
        response.ShouldNotBeNull();
        response.Data.ShouldHaveCount(2);

        response.Data.OfType<BathroomIdentifierInResponse>().Should().ContainSingle(bathroomIdentifier => bathroomIdentifier.Id == bathroom.StringId);
        response.Data.OfType<ToiletIdentifierInResponse>().Should().ContainSingle(toiletIdentifier => toiletIdentifier.Id == toilet.StringId);
    }

    // /rooms/1/relationships/residence { type: residence }
    [Fact]
    public async Task Can_set_concrete_base_resource_at_abstract_ToOne_relationship_endpoint()
    {
        // Arrange
        Bathroom existingBathroom = _fakers.Bathroom.GenerateOne();
        existingBathroom.Residence = _fakers.FamilyHome.GenerateOne();

        Mansion existingMansion = _fakers.Mansion.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Bathrooms.Add(existingBathroom);
            dbContext.Mansions.Add(existingMansion);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new OnlyRelationshipsInheritanceClient(httpClient);

        var requestBody = new ToOneResidenceInRequest
        {
            Data = new ResidenceIdentifierInRequest
            {
                Id = existingMansion.StringId
            }
        };

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PatchRoomResidenceRelationshipAsync(existingBathroom.StringId!, requestBody));

        // Assert
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Room roomInDatabase = await dbContext.Rooms.Include(room => room.Residence).FirstWithIdAsync(existingBathroom.Id);

            roomInDatabase.Should().BeOfType<Bathroom>();

            roomInDatabase.Residence.Should().BeOfType<Mansion>();
            roomInDatabase.Residence.Id.Should().Be(existingMansion.Id);
        });
    }

    // /rooms/1/relationships/residence { type: mansion }
    [Fact]
    public async Task Can_set_concrete_derived_resource_at_abstract_ToOne_relationship_endpoint()
    {
        // Arrange
        Bathroom existingBathroom = _fakers.Bathroom.GenerateOne();
        existingBathroom.Residence = _fakers.FamilyHome.GenerateOne();

        Mansion existingMansion = _fakers.Mansion.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Bathrooms.Add(existingBathroom);
            dbContext.Mansions.Add(existingMansion);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new OnlyRelationshipsInheritanceClient(httpClient);

        var requestBody = new ToOneResidenceInRequest
        {
            Data = new MansionIdentifierInRequest
            {
                Id = existingMansion.StringId
            }
        };

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PatchRoomResidenceRelationshipAsync(existingBathroom.StringId!, requestBody));

        // Assert
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Room roomInDatabase = await dbContext.Rooms.Include(room => room.Residence).FirstWithIdAsync(existingBathroom.Id);

            roomInDatabase.Should().BeOfType<Bathroom>();

            roomInDatabase.Residence.Should().BeOfType<Mansion>();
            roomInDatabase.Residence.Id.Should().Be(existingMansion.Id);
        });
    }

    // /bathrooms/1/relationships/residence { type: residence }
    [Fact]
    public async Task Can_set_concrete_base_resource_at_concrete_derived_ToOne_relationship_endpoint()
    {
        // Arrange
        Bathroom existingBathroom = _fakers.Bathroom.GenerateOne();
        existingBathroom.Residence = _fakers.FamilyHome.GenerateOne();

        Mansion existingMansion = _fakers.Mansion.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Bathrooms.Add(existingBathroom);
            dbContext.Mansions.Add(existingMansion);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new OnlyRelationshipsInheritanceClient(httpClient);

        var requestBody = new ToOneResidenceInRequest
        {
            Data = new ResidenceIdentifierInRequest
            {
                Id = existingMansion.StringId
            }
        };

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PatchBathroomResidenceRelationshipAsync(existingBathroom.StringId!, requestBody));

        // Assert
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Room roomInDatabase = await dbContext.Rooms.Include(room => room.Residence).FirstWithIdAsync(existingBathroom.Id);

            roomInDatabase.Should().BeOfType<Bathroom>();

            roomInDatabase.Residence.Should().BeOfType<Mansion>();
            roomInDatabase.Residence.Id.Should().Be(existingMansion.Id);
        });
    }

    // /bathrooms/1/relationships/residence { type: mansion }
    [Fact]
    public async Task Can_set_concrete_derived_resource_at_concrete_derived_ToOne_relationship_endpoint()
    {
        // Arrange
        Bathroom existingBathroom = _fakers.Bathroom.GenerateOne();
        existingBathroom.Residence = _fakers.FamilyHome.GenerateOne();

        Mansion existingMansion = _fakers.Mansion.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Bathrooms.Add(existingBathroom);
            dbContext.Mansions.Add(existingMansion);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new OnlyRelationshipsInheritanceClient(httpClient);

        var requestBody = new ToOneResidenceInRequest
        {
            Data = new MansionIdentifierInRequest
            {
                Id = existingMansion.StringId
            }
        };

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PatchBathroomResidenceRelationshipAsync(existingBathroom.StringId!, requestBody));

        // Assert
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Room roomInDatabase = await dbContext.Rooms.Include(room => room.Residence).FirstWithIdAsync(existingBathroom.Id);

            roomInDatabase.Should().BeOfType<Bathroom>();

            roomInDatabase.Residence.Should().BeOfType<Mansion>();
            roomInDatabase.Residence.Id.Should().Be(existingMansion.Id);
        });
    }

    // /residences/1/relationships/rooms { type: room }
    [Fact]
    public async Task Can_set_abstract_resources_at_concrete_base_ToMany_relationship_endpoint()
    {
        // Arrange
        Mansion existingMansion = _fakers.Mansion.GenerateOne();
        existingMansion.Rooms = _fakers.LivingRoom.GenerateSet<LivingRoom, Room>(1);

        Bedroom existingBedroom = _fakers.Bedroom.GenerateOne();
        existingBedroom.Residence = _fakers.FamilyHome.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Mansions.Add(existingMansion);
            dbContext.Bedrooms.Add(existingBedroom);

            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new OnlyRelationshipsInheritanceClient(httpClient);

        var requestBody = new ToManyRoomInRequest
        {
            Data =
            [
                new RoomIdentifierInRequest
                {
                    Id = existingBedroom.StringId
                }
            ]
        };

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PatchResidenceRoomsRelationshipAsync(existingMansion.StringId!, requestBody));

        // Assert
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Residence residenceInDatabase = await dbContext.Residences.Include(residence => residence.Rooms).FirstWithIdAsync(existingMansion.Id);

            residenceInDatabase.Should().BeOfType<Mansion>();

            residenceInDatabase.Rooms.ShouldHaveCount(1);
            residenceInDatabase.Rooms.OfType<Bedroom>().Should().ContainSingle(bedroomIdentifier => bedroomIdentifier.Id == existingBedroom.Id);
        });
    }

    // /residences/1/relationships/rooms { type: bedroom }
    [Fact]
    public async Task Can_set_concrete_derived_resources_at_concrete_base_ToMany_relationship_endpoint()
    {
        // Arrange
        Mansion existingMansion = _fakers.Mansion.GenerateOne();
        existingMansion.Rooms = _fakers.LivingRoom.GenerateSet<LivingRoom, Room>(1);

        Bedroom existingBedroom = _fakers.Bedroom.GenerateOne();
        existingBedroom.Residence = _fakers.FamilyHome.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Mansions.Add(existingMansion);
            dbContext.Bedrooms.Add(existingBedroom);

            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new OnlyRelationshipsInheritanceClient(httpClient);

        var requestBody = new ToManyRoomInRequest
        {
            Data =
            [
                new BedroomIdentifierInRequest
                {
                    Id = existingBedroom.StringId
                }
            ]
        };

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PatchResidenceRoomsRelationshipAsync(existingMansion.StringId!, requestBody));

        // Assert
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Residence residenceInDatabase = await dbContext.Residences.Include(residence => residence.Rooms).FirstWithIdAsync(existingMansion.Id);

            residenceInDatabase.Should().BeOfType<Mansion>();

            residenceInDatabase.Rooms.ShouldHaveCount(1);
            residenceInDatabase.Rooms.OfType<Bedroom>().Should().ContainSingle(bedroomIdentifier => bedroomIdentifier.Id == existingBedroom.Id);
        });
    }

    // /mansions/1/relationships/rooms { type: room }
    [Fact]
    public async Task Can_set_abstract_resources_at_concrete_derived_ToMany_relationship_endpoint()
    {
        // Arrange
        Mansion existingMansion = _fakers.Mansion.GenerateOne();
        existingMansion.Rooms = _fakers.LivingRoom.GenerateSet<LivingRoom, Room>(1);

        Bedroom existingBedroom = _fakers.Bedroom.GenerateOne();
        existingBedroom.Residence = _fakers.FamilyHome.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Mansions.Add(existingMansion);
            dbContext.Bedrooms.Add(existingBedroom);

            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new OnlyRelationshipsInheritanceClient(httpClient);

        var requestBody = new ToManyRoomInRequest
        {
            Data =
            [
                new RoomIdentifierInRequest
                {
                    Id = existingBedroom.StringId
                }
            ]
        };

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PatchMansionRoomsRelationshipAsync(existingMansion.StringId!, requestBody));

        // Assert
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Residence residenceInDatabase = await dbContext.Residences.Include(residence => residence.Rooms).FirstWithIdAsync(existingMansion.Id);

            residenceInDatabase.Should().BeOfType<Mansion>();

            residenceInDatabase.Rooms.ShouldHaveCount(1);
            residenceInDatabase.Rooms.OfType<Bedroom>().Should().ContainSingle(bedroomIdentifier => bedroomIdentifier.Id == existingBedroom.Id);
        });
    }

    // /mansions/1/relationships/rooms { type: bedroom }
    [Fact]
    public async Task Can_set_concrete_derived_resources_at_concrete_derived_ToMany_relationship_endpoint()
    {
        // Arrange
        Mansion existingMansion = _fakers.Mansion.GenerateOne();
        existingMansion.Rooms = _fakers.LivingRoom.GenerateSet<LivingRoom, Room>(1);

        Bedroom existingBedroom = _fakers.Bedroom.GenerateOne();
        existingBedroom.Residence = _fakers.FamilyHome.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Mansions.Add(existingMansion);
            dbContext.Bedrooms.Add(existingBedroom);

            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new OnlyRelationshipsInheritanceClient(httpClient);

        var requestBody = new ToManyRoomInRequest
        {
            Data =
            [
                new BedroomIdentifierInRequest
                {
                    Id = existingBedroom.StringId
                }
            ]
        };

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PatchMansionRoomsRelationshipAsync(existingMansion.StringId!, requestBody));

        // Assert
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Residence residenceInDatabase = await dbContext.Residences.Include(residence => residence.Rooms).FirstWithIdAsync(existingMansion.Id);

            residenceInDatabase.Should().BeOfType<Mansion>();

            residenceInDatabase.Rooms.ShouldHaveCount(1);
            residenceInDatabase.Rooms.OfType<Bedroom>().Should().ContainSingle(bedroomIdentifier => bedroomIdentifier.Id == existingBedroom.Id);
        });
    }

    // TODO: Add tests for add/remove to-many
    // TODO: Add tests for operations
    // TODO: Remove comments above [Fact]

    // [to-one]
    // Room.Residence

    // [to-many]
    // Residence.Rooms
    // Mansion.Staff
    // District.Buildings

    public void Dispose()
    {
        _logHttpMessageHandler.Dispose();
    }
}
