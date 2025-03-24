using FluentAssertions;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Kiota.Http.HttpClientLibrary;
using OpenApiKiotaEndToEndTests.ResourceInheritance.OnlyRelationships.GeneratedCode;
using OpenApiKiotaEndToEndTests.ResourceInheritance.OnlyRelationships.GeneratedCode.Models;
using OpenApiTests;
using OpenApiTests.ResourceInheritance;
using OpenApiTests.ResourceInheritance.Models;
using OpenApiTests.ResourceInheritance.OnlyRelationships;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiKiotaEndToEndTests.ResourceInheritance.OnlyRelationships;

public sealed class RelationshipInheritanceTests
    : IClassFixture<IntegrationTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext>>, IDisposable
{
    private readonly IntegrationTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> _testContext;
    private readonly TestableHttpClientRequestAdapterFactory _requestAdapterFactory;
    private readonly ResourceInheritanceFakers _fakers = new();

    public RelationshipInheritanceTests(IntegrationTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> testContext,
        ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _requestAdapterFactory = new TestableHttpClientRequestAdapterFactory(testOutputHelper);

        testContext.UseInheritanceControllers(true);

        testContext.ConfigureServices(services =>
        {
            services.AddSingleton<IJsonApiEndpointFilter, OnlyRelationshipsEndpointFilter>();
            services.AddSingleton<IAtomicOperationFilter, OnlyRelationshipsOperationFilter>();
        });
    }

    // GET /rooms/1/relationships/residence => familyHome
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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new OnlyRelationshipsInheritanceClient(requestAdapter);

        // Act
        ResidenceIdentifierResponseDocument? response = await apiClient.Rooms[bedroom.StringId!].Relationships.Residence.GetAsync();

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().BeOfType<FamilyHomeIdentifierInResponse>();
        response.Data.Id.Should().Be(bedroom.Residence.StringId);
    }

    // GET /bedrooms/1/relationships/residence => familyHome
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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new OnlyRelationshipsInheritanceClient(requestAdapter);

        // Act
        ResidenceIdentifierResponseDocument? response = await apiClient.Bedrooms[bedroom.StringId!].Relationships.Residence.GetAsync();

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().BeOfType<FamilyHomeIdentifierInResponse>();
        response.Data.Id.Should().Be(bedroom.Residence.StringId);
    }

    // GET /residences/1/relationships/rooms => [kitchen, bedroom]
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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new OnlyRelationshipsInheritanceClient(requestAdapter);

        // Act
        RoomIdentifierCollectionResponseDocument? response = await apiClient.Residences[familyHome.StringId!].Relationships.Rooms.GetAsync();

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().HaveCount(2);

        response.Data.OfType<KitchenIdentifierInResponse>().Should().ContainSingle(data => data.Id == kitchen.StringId);
        response.Data.OfType<BedroomIdentifierInResponse>().Should().ContainSingle(data => data.Id == bedroom.StringId);
    }

    // GET /mansions/1/relationships/rooms => [bathroom, toilet]
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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new OnlyRelationshipsInheritanceClient(requestAdapter);

        // Act
        RoomIdentifierCollectionResponseDocument? response = await apiClient.Mansions[mansion.StringId!].Relationships.Rooms.GetAsync();

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().HaveCount(2);

        response.Data.OfType<BathroomIdentifierInResponse>().Should().ContainSingle(data => data.Id == bathroom.StringId);
        response.Data.OfType<ToiletIdentifierInResponse>().Should().ContainSingle(data => data.Id == toilet.StringId);
    }

    // PATCH /rooms/1/relationships/residence { type: residence }
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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new OnlyRelationshipsInheritanceClient(requestAdapter);

        var requestBody = new ToOneResidenceInRequest
        {
            Data = new ResidenceIdentifierInRequest
            {
                Type = ResourceType.Residences,
                Id = existingMansion.StringId
            }
        };

        // Act
        await apiClient.Rooms[existingBathroom.StringId!].Relationships.Residence.PatchAsync(requestBody);

        // Assert
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Room roomInDatabase = await dbContext.Rooms.Include(room => room.Residence).FirstWithIdAsync(existingBathroom.Id);

            roomInDatabase.Should().BeOfType<Bathroom>();

            roomInDatabase.Residence.Should().BeOfType<Mansion>();
            roomInDatabase.Residence.Id.Should().Be(existingMansion.Id);
        });
    }

    // PATCH /rooms/1/relationships/residence { type: mansion }
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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new OnlyRelationshipsInheritanceClient(requestAdapter);

        var requestBody = new ToOneResidenceInRequest
        {
            Data = new MansionIdentifierInRequest
            {
                Type = ResourceType.Mansions,
                Id = existingMansion.StringId
            }
        };

        // Act
        await apiClient.Rooms[existingBathroom.StringId!].Relationships.Residence.PatchAsync(requestBody);

        // Assert
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Room roomInDatabase = await dbContext.Rooms.Include(room => room.Residence).FirstWithIdAsync(existingBathroom.Id);

            roomInDatabase.Should().BeOfType<Bathroom>();

            roomInDatabase.Residence.Should().BeOfType<Mansion>();
            roomInDatabase.Residence.Id.Should().Be(existingMansion.Id);
        });
    }

    // PATCH /bathrooms/1/relationships/residence { type: residence }
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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new OnlyRelationshipsInheritanceClient(requestAdapter);

        var requestBody = new ToOneResidenceInRequest
        {
            Data = new ResidenceIdentifierInRequest
            {
                Type = ResourceType.Residences,
                Id = existingMansion.StringId
            }
        };

        // Act
        await apiClient.Bathrooms[existingBathroom.StringId!].Relationships.Residence.PatchAsync(requestBody);

        // Assert
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Room roomInDatabase = await dbContext.Rooms.Include(room => room.Residence).FirstWithIdAsync(existingBathroom.Id);

            roomInDatabase.Should().BeOfType<Bathroom>();

            roomInDatabase.Residence.Should().BeOfType<Mansion>();
            roomInDatabase.Residence.Id.Should().Be(existingMansion.Id);
        });
    }

    // PATCH /bathrooms/1/relationships/residence { type: mansion }
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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new OnlyRelationshipsInheritanceClient(requestAdapter);

        var requestBody = new ToOneResidenceInRequest
        {
            Data = new MansionIdentifierInRequest
            {
                Type = ResourceType.Mansions,
                Id = existingMansion.StringId
            }
        };

        // Act
        await apiClient.Bathrooms[existingBathroom.StringId!].Relationships.Residence.PatchAsync(requestBody);

        // Assert
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Room roomInDatabase = await dbContext.Rooms.Include(room => room.Residence).FirstWithIdAsync(existingBathroom.Id);

            roomInDatabase.Should().BeOfType<Bathroom>();

            roomInDatabase.Residence.Should().BeOfType<Mansion>();
            roomInDatabase.Residence.Id.Should().Be(existingMansion.Id);
        });
    }

    // PATCH /residences/1/relationships/rooms { type: room }
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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new OnlyRelationshipsInheritanceClient(requestAdapter);

        var requestBody = new ToManyRoomInRequest
        {
            Data =
            [
                new RoomIdentifierInRequest
                {
                    Type = ResourceType.Rooms,
                    Id = existingBedroom.StringId
                }
            ]
        };

        // Act
        await apiClient.Residences[existingMansion.StringId!].Relationships.Rooms.PatchAsync(requestBody);

        // Assert
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Residence residenceInDatabase = await dbContext.Residences.Include(residence => residence.Rooms).FirstWithIdAsync(existingMansion.Id);

            residenceInDatabase.Should().BeOfType<Mansion>();

            residenceInDatabase.Rooms.Should().HaveCount(1);
            residenceInDatabase.Rooms.OfType<Bedroom>().Should().ContainSingle(bedroom => bedroom.Id == existingBedroom.Id);
        });
    }

    // PATCH /residences/1/relationships/rooms { type: bedroom }
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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new OnlyRelationshipsInheritanceClient(requestAdapter);

        var requestBody = new ToManyRoomInRequest
        {
            Data =
            [
                new BedroomIdentifierInRequest
                {
                    Type = ResourceType.Bedrooms,
                    Id = existingBedroom.StringId
                }
            ]
        };

        // Act
        await apiClient.Residences[existingMansion.StringId!].Relationships.Rooms.PatchAsync(requestBody);

        // Assert
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Residence residenceInDatabase = await dbContext.Residences.Include(residence => residence.Rooms).FirstWithIdAsync(existingMansion.Id);

            residenceInDatabase.Should().BeOfType<Mansion>();

            residenceInDatabase.Rooms.Should().HaveCount(1);
            residenceInDatabase.Rooms.OfType<Bedroom>().Should().ContainSingle(bedroom => bedroom.Id == existingBedroom.Id);
        });
    }

    // PATCH /mansions/1/relationships/rooms { type: room }
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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new OnlyRelationshipsInheritanceClient(requestAdapter);

        var requestBody = new ToManyRoomInRequest
        {
            Data =
            [
                new RoomIdentifierInRequest
                {
                    Type = ResourceType.Rooms,
                    Id = existingBedroom.StringId
                }
            ]
        };

        // Act
        await apiClient.Mansions[existingMansion.StringId!].Relationships.Rooms.PatchAsync(requestBody);

        // Assert
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Residence residenceInDatabase = await dbContext.Residences.Include(residence => residence.Rooms).FirstWithIdAsync(existingMansion.Id);

            residenceInDatabase.Should().BeOfType<Mansion>();

            residenceInDatabase.Rooms.Should().HaveCount(1);
            residenceInDatabase.Rooms.OfType<Bedroom>().Should().ContainSingle(bedroom => bedroom.Id == existingBedroom.Id);
        });
    }

    // PATCH /mansions/1/relationships/rooms { type: bedroom }
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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new OnlyRelationshipsInheritanceClient(requestAdapter);

        var requestBody = new ToManyRoomInRequest
        {
            Data =
            [
                new BedroomIdentifierInRequest
                {
                    Type = ResourceType.Bedrooms,
                    Id = existingBedroom.StringId
                }
            ]
        };

        // Act
        await apiClient.Mansions[existingMansion.StringId!].Relationships.Rooms.PatchAsync(requestBody);

        // Assert
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Residence residenceInDatabase = await dbContext.Residences.Include(residence => residence.Rooms).FirstWithIdAsync(existingMansion.Id);

            residenceInDatabase.Should().BeOfType<Mansion>();

            residenceInDatabase.Rooms.Should().HaveCount(1);
            residenceInDatabase.Rooms.OfType<Bedroom>().Should().ContainSingle(bedroom => bedroom.Id == existingBedroom.Id);
        });
    }

    // POST /residences/1/relationships/rooms { type: room }
    [Fact]
    public async Task Can_add_abstract_resource_at_concrete_base_ToMany_relationship_endpoint()
    {
        // Arrange
        Mansion existingMansion = _fakers.Mansion.GenerateOne();
        LivingRoom existingLivingRoom = _fakers.LivingRoom.GenerateOne();
        existingMansion.Rooms.Add(existingLivingRoom);

        Bedroom existingBedroom = _fakers.Bedroom.GenerateOne();
        existingBedroom.Residence = _fakers.FamilyHome.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Mansions.Add(existingMansion);
            dbContext.Bedrooms.Add(existingBedroom);

            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new OnlyRelationshipsInheritanceClient(requestAdapter);

        var requestBody = new ToManyRoomInRequest
        {
            Data =
            [
                new RoomIdentifierInRequest
                {
                    Type = ResourceType.Rooms,
                    Id = existingBedroom.StringId
                }
            ]
        };

        // Act
        await apiClient.Residences[existingMansion.StringId!].Relationships.Rooms.PostAsync(requestBody);

        // Assert
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Residence residenceInDatabase = await dbContext.Residences.Include(residence => residence.Rooms).FirstWithIdAsync(existingMansion.Id);

            residenceInDatabase.Should().BeOfType<Mansion>();

            residenceInDatabase.Rooms.Should().HaveCount(2);
            residenceInDatabase.Rooms.OfType<LivingRoom>().Should().ContainSingle(livingRoom => livingRoom.Id == existingLivingRoom.Id);
            residenceInDatabase.Rooms.OfType<Bedroom>().Should().ContainSingle(bedroom => bedroom.Id == existingBedroom.Id);
        });
    }

    // POST /residences/1/relationships/rooms { type: bedroom }
    [Fact]
    public async Task Can_add_concrete_derived_resource_at_concrete_base_ToMany_relationship_endpoint()
    {
        // Arrange
        Mansion existingMansion = _fakers.Mansion.GenerateOne();
        LivingRoom existingLivingRoom = _fakers.LivingRoom.GenerateOne();
        existingMansion.Rooms.Add(existingLivingRoom);

        Bedroom existingBedroom = _fakers.Bedroom.GenerateOne();
        existingBedroom.Residence = _fakers.FamilyHome.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Mansions.Add(existingMansion);
            dbContext.Bedrooms.Add(existingBedroom);

            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new OnlyRelationshipsInheritanceClient(requestAdapter);

        var requestBody = new ToManyRoomInRequest
        {
            Data =
            [
                new BedroomIdentifierInRequest
                {
                    Type = ResourceType.Bedrooms,
                    Id = existingBedroom.StringId
                }
            ]
        };

        // Act
        await apiClient.Residences[existingMansion.StringId!].Relationships.Rooms.PostAsync(requestBody);

        // Assert
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Residence residenceInDatabase = await dbContext.Residences.Include(residence => residence.Rooms).FirstWithIdAsync(existingMansion.Id);

            residenceInDatabase.Should().BeOfType<Mansion>();

            residenceInDatabase.Rooms.Should().HaveCount(2);
            residenceInDatabase.Rooms.OfType<LivingRoom>().Should().ContainSingle(livingRoom => livingRoom.Id == existingLivingRoom.Id);
            residenceInDatabase.Rooms.OfType<Bedroom>().Should().ContainSingle(bedroom => bedroom.Id == existingBedroom.Id);
        });
    }

    // POST /mansions/1/relationships/rooms { type: room }
    [Fact]
    public async Task Can_add_abstract_resource_at_concrete_derived_ToMany_relationship_endpoint()
    {
        // Arrange
        Mansion existingMansion = _fakers.Mansion.GenerateOne();
        LivingRoom existingLivingRoom = _fakers.LivingRoom.GenerateOne();
        existingMansion.Rooms.Add(existingLivingRoom);

        Bedroom existingBedroom = _fakers.Bedroom.GenerateOne();
        existingBedroom.Residence = _fakers.FamilyHome.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Mansions.Add(existingMansion);
            dbContext.Bedrooms.Add(existingBedroom);

            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new OnlyRelationshipsInheritanceClient(requestAdapter);

        var requestBody = new ToManyRoomInRequest
        {
            Data =
            [
                new RoomIdentifierInRequest
                {
                    Type = ResourceType.Bedrooms,
                    Id = existingBedroom.StringId
                }
            ]
        };

        // Act
        await apiClient.Mansions[existingMansion.StringId!].Relationships.Rooms.PostAsync(requestBody);

        // Assert
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Residence residenceInDatabase = await dbContext.Residences.Include(residence => residence.Rooms).FirstWithIdAsync(existingMansion.Id);

            residenceInDatabase.Should().BeOfType<Mansion>();

            residenceInDatabase.Rooms.Should().HaveCount(2);
            residenceInDatabase.Rooms.OfType<LivingRoom>().Should().ContainSingle(livingRoom => livingRoom.Id == existingLivingRoom.Id);
            residenceInDatabase.Rooms.OfType<Bedroom>().Should().ContainSingle(bedroom => bedroom.Id == existingBedroom.Id);
        });
    }

    // POST /mansions/1/relationships/rooms { type: bedroom }
    [Fact]
    public async Task Can_add_concrete_derived_resource_at_concrete_derived_ToMany_relationship_endpoint()
    {
        // Arrange
        Mansion existingMansion = _fakers.Mansion.GenerateOne();
        LivingRoom existingLivingRoom = _fakers.LivingRoom.GenerateOne();
        existingMansion.Rooms.Add(existingLivingRoom);

        Bedroom existingBedroom = _fakers.Bedroom.GenerateOne();
        existingBedroom.Residence = _fakers.FamilyHome.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Mansions.Add(existingMansion);
            dbContext.Bedrooms.Add(existingBedroom);

            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new OnlyRelationshipsInheritanceClient(requestAdapter);

        var requestBody = new ToManyRoomInRequest
        {
            Data =
            [
                new BedroomIdentifierInRequest
                {
                    Type = ResourceType.Bedrooms,
                    Id = existingBedroom.StringId
                }
            ]
        };

        // Act
        await apiClient.Mansions[existingMansion.StringId!].Relationships.Rooms.PostAsync(requestBody);

        // Assert
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Residence residenceInDatabase = await dbContext.Residences.Include(residence => residence.Rooms).FirstWithIdAsync(existingMansion.Id);

            residenceInDatabase.Should().BeOfType<Mansion>();

            residenceInDatabase.Rooms.Should().HaveCount(2);
            residenceInDatabase.Rooms.OfType<LivingRoom>().Should().ContainSingle(livingRoom => livingRoom.Id == existingLivingRoom.Id);
            residenceInDatabase.Rooms.OfType<Bedroom>().Should().ContainSingle(bedroom => bedroom.Id == existingBedroom.Id);
        });
    }

    // DELETE /residences/1/relationships/rooms { type: room }
    [Fact]
    public async Task Can_remove_abstract_resource_at_concrete_base_ToMany_relationship_endpoint()
    {
        // Arrange
        Mansion existingMansion = _fakers.Mansion.GenerateOne();
        existingMansion.Rooms = _fakers.LivingRoom.GenerateSet<LivingRoom, Room>(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Mansions.Add(existingMansion);

            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new OnlyRelationshipsInheritanceClient(requestAdapter);

        var requestBody = new ToManyRoomInRequest
        {
            Data =
            [
                new RoomIdentifierInRequest
                {
                    Type = ResourceType.Rooms,
                    Id = existingMansion.Rooms.ElementAt(0).StringId
                }
            ]
        };

        // Act
        await apiClient.Residences[existingMansion.StringId!].Relationships.Rooms.DeleteAsync(requestBody);

        // Assert
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Residence residenceInDatabase = await dbContext.Residences.Include(residence => residence.Rooms).FirstWithIdAsync(existingMansion.Id);

            residenceInDatabase.Should().BeOfType<Mansion>();

            residenceInDatabase.Rooms.Should().BeEmpty();
        });
    }

    // DELETE /residences/1/relationships/rooms { type: bedroom }
    [Fact]
    public async Task Can_remove_concrete_derived_resource_at_concrete_base_ToMany_relationship_endpoint()
    {
        // Arrange
        Mansion existingMansion = _fakers.Mansion.GenerateOne();
        existingMansion.Rooms = _fakers.Bedroom.GenerateSet<Bedroom, Room>(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Mansions.Add(existingMansion);

            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new OnlyRelationshipsInheritanceClient(requestAdapter);

        var requestBody = new ToManyRoomInRequest
        {
            Data =
            [
                new BedroomIdentifierInRequest
                {
                    Type = ResourceType.Bedrooms,
                    Id = existingMansion.Rooms.ElementAt(0).StringId
                }
            ]
        };

        // Act
        await apiClient.Residences[existingMansion.StringId!].Relationships.Rooms.DeleteAsync(requestBody);

        // Assert
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Residence residenceInDatabase = await dbContext.Residences.Include(residence => residence.Rooms).FirstWithIdAsync(existingMansion.Id);

            residenceInDatabase.Should().BeOfType<Mansion>();

            residenceInDatabase.Rooms.Should().BeEmpty();
        });
    }

    // DELETE /mansions/1/relationships/rooms { type: room }
    [Fact]
    public async Task Can_remove_abstract_resource_at_concrete_derived_ToMany_relationship_endpoint()
    {
        // Arrange
        Mansion existingMansion = _fakers.Mansion.GenerateOne();
        existingMansion.Rooms = _fakers.LivingRoom.GenerateSet<LivingRoom, Room>(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Mansions.Add(existingMansion);

            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new OnlyRelationshipsInheritanceClient(requestAdapter);

        var requestBody = new ToManyRoomInRequest
        {
            Data =
            [
                new RoomIdentifierInRequest
                {
                    Type = ResourceType.Rooms,
                    Id = existingMansion.Rooms.ElementAt(0).StringId
                }
            ]
        };

        // Act
        await apiClient.Mansions[existingMansion.StringId!].Relationships.Rooms.DeleteAsync(requestBody);

        // Assert
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Residence residenceInDatabase = await dbContext.Residences.Include(residence => residence.Rooms).FirstWithIdAsync(existingMansion.Id);

            residenceInDatabase.Should().BeOfType<Mansion>();

            residenceInDatabase.Rooms.Should().BeEmpty();
        });
    }

    // DELETE /mansions/1/relationships/rooms { type: bedroom }
    [Fact]
    public async Task Can_remove_concrete_derived_resource_at_concrete_derived_ToMany_relationship_endpoint()
    {
        // Arrange
        Mansion existingMansion = _fakers.Mansion.GenerateOne();
        existingMansion.Rooms = _fakers.Bedroom.GenerateSet<Bedroom, Room>(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Mansions.Add(existingMansion);

            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new OnlyRelationshipsInheritanceClient(requestAdapter);

        var requestBody = new ToManyRoomInRequest
        {
            Data =
            [
                new BedroomIdentifierInRequest
                {
                    Type = ResourceType.Bedrooms,
                    Id = existingMansion.Rooms.ElementAt(0).StringId
                }
            ]
        };

        // Act
        await apiClient.Mansions[existingMansion.StringId!].Relationships.Rooms.DeleteAsync(requestBody);

        // Assert
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Residence residenceInDatabase = await dbContext.Residences.Include(residence => residence.Rooms).FirstWithIdAsync(existingMansion.Id);

            residenceInDatabase.Should().BeOfType<Mansion>();

            residenceInDatabase.Rooms.Should().BeEmpty();
        });
    }

    public void Dispose()
    {
        _requestAdapterFactory.Dispose();
    }
}
