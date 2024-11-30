using FluentAssertions;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Kiota.Http.HttpClientLibrary;
using OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode;
using OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Models;
using OpenApiTests;
using OpenApiTests.ResourceInheritance;
using OpenApiTests.ResourceInheritance.Models;
using OpenApiTests.ResourceInheritance.NoOperations;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations;

public sealed class NoOperationsInheritanceTests
    : IClassFixture<IntegrationTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext>>, IDisposable
{
    private readonly IntegrationTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> _testContext;
    private readonly TestableHttpClientRequestAdapterFactory _requestAdapterFactory;
    private readonly ResourceInheritanceFakers _fakers = new();

    public NoOperationsInheritanceTests(IntegrationTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> testContext,
        ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _requestAdapterFactory = new TestableHttpClientRequestAdapterFactory(testOutputHelper);

        testContext.UseInheritanceControllers(false);

        testContext.ConfigureServices(services =>
        {
            services.AddScoped(typeof(IResourceChangeTracker<>), typeof(NeverSameResourceChangeTracker<>));

            services.AddSingleton<IJsonApiEndpointFilter, NoOperationsEndpointFilter>();
            services.AddSingleton<IAtomicOperationFilter, NoOperationsOperationFilter>();
        });
    }

    [Fact]
    public async Task Can_create_concrete_base_resource_at_abstract_endpoint()
    {
        // Arrange
        double newLengthInMeters = (double)_fakers.Road.GenerateOne().LengthInMeters;

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new NoOperationsInheritanceClient(requestAdapter);

        CreateRoadRequestDocument requestBody = new()
        {
            Data = new DataInCreateRoadRequest
            {
                Type = ResourceType.Roads,
                Attributes = new AttributesInCreateRoadRequest
                {
                    LengthInMeters = newLengthInMeters
                }
            }
        };

        // Act
        RoadPrimaryResponseDocument? response = await apiClient.Roads.PostAsync(requestBody);

        // Assert
        response.ShouldNotBeNull();

        RoadDataInResponse roadData = response.Data.Should().BeOfType<RoadDataInResponse>().Subject;

        RoadAttributesInResponse roadAttributes = roadData.Attributes.Should().BeOfType<RoadAttributesInResponse>().Subject;
        roadAttributes.LengthInMeters.Should().Be(newLengthInMeters);

        roadData.Relationships.Should().BeNull();

        long newRoadId = long.Parse(roadData.Id.ShouldNotBeNull());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Road roadInDatabase = await dbContext.Roads.FirstWithIdAsync(newRoadId);

            roadInDatabase.LengthInMeters.Should().Be((decimal)newLengthInMeters);
        });
    }

    [Fact]
    public async Task Can_create_concrete_derived_resource_at_abstract_endpoint()
    {
        // Arrange
        Bedroom existingBedroom1 = _fakers.Bedroom.GenerateOne();
        Bedroom existingBedroom2 = _fakers.Bedroom.GenerateOne();
        LivingRoom existingLivingRoom = _fakers.LivingRoom.GenerateOne();

        Mansion existingMansion = _fakers.Mansion.GenerateOne();
        existingMansion.Rooms.Add(existingBedroom1);
        existingMansion.Rooms.Add(existingBedroom2);
        existingMansion.Rooms.Add(existingLivingRoom);

        FamilyHome newFamilyHome = _fakers.FamilyHome.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Mansions.Add(existingMansion);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new NoOperationsInheritanceClient(requestAdapter);

        CreateBuildingRequestDocument requestBody = new()
        {
            Data = new DataInCreateFamilyHomeRequest
            {
                Type = ResourceType.FamilyHomes,
                Attributes = new AttributesInCreateFamilyHomeRequest
                {
                    SurfaceInSquareMeters = newFamilyHome.SurfaceInSquareMeters!.Value,
                    NumberOfResidents = newFamilyHome.NumberOfResidents!.Value,
                    FloorCount = newFamilyHome.FloorCount
                },
                Relationships = new RelationshipsInCreateFamilyHomeRequest
                {
                    Rooms = new ToManyRoomInRequest
                    {
                        Data =
                        [
                            new RoomIdentifierInRequest
                            {
                                Type = ResourceType.Bedrooms,
                                Id = existingBedroom1.StringId!
                            },
                            new RoomIdentifierInRequest
                            {
                                Type = ResourceType.Bedrooms,
                                Id = existingBedroom2.StringId!
                            },
                            new RoomIdentifierInRequest
                            {
                                Type = ResourceType.LivingRooms,
                                Id = existingLivingRoom.StringId!
                            }
                        ]
                    }
                }
            }
        };

        // Act
        BuildingPrimaryResponseDocument? response = await apiClient.Buildings.PostAsync(requestBody);

        // Assert
        response.ShouldNotBeNull();

        FamilyHomeDataInResponse familyHomeData = response.Data.Should().BeOfType<FamilyHomeDataInResponse>().Subject;

        FamilyHomeAttributesInResponse familyHomeAttributes = familyHomeData.Attributes.Should().BeOfType<FamilyHomeAttributesInResponse>().Subject;
        familyHomeAttributes.SurfaceInSquareMeters.Should().Be(newFamilyHome.SurfaceInSquareMeters);
        familyHomeAttributes.NumberOfResidents.Should().Be(newFamilyHome.NumberOfResidents);
        familyHomeAttributes.FloorCount.Should().Be(newFamilyHome.FloorCount);

        FamilyHomeRelationshipsInResponse familyHomeRelationships = familyHomeData.Relationships.Should().BeOfType<FamilyHomeRelationshipsInResponse>().Subject;
        familyHomeRelationships.Rooms.Should().NotBeNull();

        long newFamilyHomeId = long.Parse(familyHomeData.Id.ShouldNotBeNull());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            FamilyHome familyHomeInDatabase = await dbContext.FamilyHomes.Include(familyHome => familyHome.Rooms).FirstWithIdAsync(newFamilyHomeId);

            familyHomeInDatabase.SurfaceInSquareMeters.Should().Be(newFamilyHome.SurfaceInSquareMeters);
            familyHomeInDatabase.NumberOfResidents.Should().Be(newFamilyHome.NumberOfResidents);
            familyHomeInDatabase.FloorCount.Should().Be(newFamilyHome.FloorCount);

            familyHomeInDatabase.Rooms.ShouldHaveCount(3);
            familyHomeInDatabase.Rooms.OfType<Bedroom>().Should().ContainSingle(bedroom => bedroom.Id == existingBedroom1.Id);
            familyHomeInDatabase.Rooms.OfType<Bedroom>().Should().ContainSingle(bedroom => bedroom.Id == existingBedroom2.Id);
            familyHomeInDatabase.Rooms.OfType<LivingRoom>().Should().ContainSingle(livingRoom => livingRoom.Id == existingLivingRoom.Id);
        });
    }

    [Fact]
    public async Task Can_update_concrete_base_resource_at_abstract_endpoint()
    {
        // Arrange
        Road existingRoad = _fakers.Road.GenerateOne();

        double newLengthInMeters = (double)_fakers.Road.GenerateOne().LengthInMeters;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Roads.Add(existingRoad);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new NoOperationsInheritanceClient(requestAdapter);

        UpdateRoadRequestDocument requestBody = new()
        {
            Data = new DataInUpdateRoadRequest
            {
                Type = ResourceType.Roads,
                Id = existingRoad.StringId!,
                Attributes = new AttributesInUpdateRoadRequest
                {
                    LengthInMeters = newLengthInMeters
                }
            }
        };

        // Act
        RoadPrimaryResponseDocument? response = await apiClient.Roads[existingRoad.StringId].PatchAsync(requestBody);

        // Assert
        response.ShouldNotBeNull();

        RoadDataInResponse roadData = response.Data.Should().BeOfType<RoadDataInResponse>().Subject;

        RoadAttributesInResponse roadAttributes = roadData.Attributes.Should().BeOfType<RoadAttributesInResponse>().Subject;
        roadAttributes.LengthInMeters.Should().Be(newLengthInMeters);

        roadData.Relationships.Should().BeNull();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Road roadInDatabase = await dbContext.Roads.FirstWithIdAsync(existingRoad.Id);

            roadInDatabase.LengthInMeters.Should().Be((decimal)newLengthInMeters);
        });
    }

    [Fact]
    public async Task Can_update_concrete_derived_resource_at_abstract_endpoint()
    {
        // Arrange
        Bedroom existingBedroom1 = _fakers.Bedroom.GenerateOne();
        Bedroom existingBedroom2 = _fakers.Bedroom.GenerateOne();
        LivingRoom existingLivingRoom = _fakers.LivingRoom.GenerateOne();

        Mansion existingMansion = _fakers.Mansion.GenerateOne();
        existingMansion.Rooms.Add(existingBedroom1);
        existingMansion.Rooms.Add(existingBedroom2);
        existingMansion.Rooms.Add(existingLivingRoom);

        FamilyHome existingFamilyHome = _fakers.FamilyHome.GenerateOne();
        existingFamilyHome.Rooms.Add(_fakers.Kitchen.GenerateOne());

        FamilyHome newFamilyHome = _fakers.FamilyHome.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Mansions.Add(existingMansion);
            dbContext.FamilyHomes.Add(existingFamilyHome);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new NoOperationsInheritanceClient(requestAdapter);

        UpdateBuildingRequestDocument requestBody = new()
        {
            Data = new DataInUpdateFamilyHomeRequest
            {
                Type = ResourceType.FamilyHomes,
                Id = existingFamilyHome.StringId!,
                Attributes = new AttributesInUpdateFamilyHomeRequest
                {
                    SurfaceInSquareMeters = newFamilyHome.SurfaceInSquareMeters!.Value,
                    NumberOfResidents = newFamilyHome.NumberOfResidents!.Value,
                    FloorCount = newFamilyHome.FloorCount
                },
                Relationships = new RelationshipsInUpdateFamilyHomeRequest
                {
                    Rooms = new ToManyRoomInRequest
                    {
                        Data =
                        [
                            new RoomIdentifierInRequest
                            {
                                Type = ResourceType.Bedrooms,
                                Id = existingBedroom1.StringId!
                            },
                            new RoomIdentifierInRequest
                            {
                                Type = ResourceType.Bedrooms,
                                Id = existingBedroom2.StringId!
                            },
                            new RoomIdentifierInRequest
                            {
                                Type = ResourceType.LivingRooms,
                                Id = existingLivingRoom.StringId!
                            }
                        ]
                    }
                }
            }
        };

        // Act
        BuildingPrimaryResponseDocument? response = await apiClient.Buildings[existingFamilyHome.StringId].PatchAsync(requestBody);

        // Assert
        response.ShouldNotBeNull();

        FamilyHomeDataInResponse familyHomeData = response.Data.Should().BeOfType<FamilyHomeDataInResponse>().Subject;

        FamilyHomeAttributesInResponse familyHomeAttributes = familyHomeData.Attributes.Should().BeOfType<FamilyHomeAttributesInResponse>().Subject;
        familyHomeAttributes.SurfaceInSquareMeters.Should().Be(newFamilyHome.SurfaceInSquareMeters);
        familyHomeAttributes.NumberOfResidents.Should().Be(newFamilyHome.NumberOfResidents);
        familyHomeAttributes.FloorCount.Should().Be(newFamilyHome.FloorCount);

        FamilyHomeRelationshipsInResponse familyHomeRelationships = familyHomeData.Relationships.Should().BeOfType<FamilyHomeRelationshipsInResponse>().Subject;
        familyHomeRelationships.Rooms.Should().NotBeNull();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            FamilyHome familyHomeInDatabase = await dbContext.FamilyHomes.Include(familyHome => familyHome.Rooms).FirstWithIdAsync(existingFamilyHome.Id);

            familyHomeInDatabase.SurfaceInSquareMeters.Should().Be(newFamilyHome.SurfaceInSquareMeters);
            familyHomeInDatabase.NumberOfResidents.Should().Be(newFamilyHome.NumberOfResidents);
            familyHomeInDatabase.FloorCount.Should().Be(newFamilyHome.FloorCount);

            familyHomeInDatabase.Rooms.ShouldHaveCount(3);
            familyHomeInDatabase.Rooms.OfType<Bedroom>().Should().ContainSingle(bedroom => bedroom.Id == existingBedroom1.Id);
            familyHomeInDatabase.Rooms.OfType<Bedroom>().Should().ContainSingle(bedroom => bedroom.Id == existingBedroom2.Id);
            familyHomeInDatabase.Rooms.OfType<LivingRoom>().Should().ContainSingle(livingRoom => livingRoom.Id == existingLivingRoom.Id);
        });
    }

    public void Dispose()
    {
        _requestAdapterFactory.Dispose();
    }
}
