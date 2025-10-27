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

public sealed class ResourceInheritanceTests
    : IClassFixture<IntegrationTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext>>, IDisposable
{
    private readonly IntegrationTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> _testContext;
    private readonly TestableHttpClientRequestAdapterFactory _requestAdapterFactory;
    private readonly ResourceInheritanceFakers _fakers = new();

    public ResourceInheritanceTests(IntegrationTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> testContext,
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
    public async Task Can_get_primary_resources_at_abstract_endpoint()
    {
        // Arrange
        Residence residence = _fakers.Residence.GenerateOne();
        Mansion mansion = _fakers.Mansion.GenerateOne();
        FamilyHome familyHome = _fakers.FamilyHome.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Building>();
            dbContext.Buildings.AddRange(residence, mansion, familyHome);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new NoOperationsInheritanceClient(requestAdapter);

        // Act
        BuildingCollectionResponseDocument? response = await apiClient.Buildings.GetAsync();

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().HaveCount(3);

        response.Data.OfType<DataInResidenceResponse>().Should().ContainSingle(data => data.Id == residence.StringId).Subject.With(data =>
        {
            AttributesInResidenceResponse attributes = data.Attributes.Should().BeOfType<AttributesInResidenceResponse>().Subject;

            attributes.SurfaceInSquareMeters.Should().Be(residence.SurfaceInSquareMeters);
            attributes.NumberOfResidents.Should().Be(residence.NumberOfResidents);

            RelationshipsInResidenceResponse relationships = data.Relationships.Should().BeOfType<RelationshipsInResidenceResponse>().Subject;

            relationships.Rooms.Should().NotBeNull();
            relationships.Rooms.Data.Should().BeNull();
        });

        response.Data.OfType<DataInMansionResponse>().Should().ContainSingle(data => data.Id == mansion.StringId).Subject.With(data =>
        {
            AttributesInMansionResponse attributes = data.Attributes.Should().BeOfType<AttributesInMansionResponse>().Subject;

            attributes.SurfaceInSquareMeters.Should().Be(mansion.SurfaceInSquareMeters);
            attributes.NumberOfResidents.Should().Be(mansion.NumberOfResidents);
            attributes.OwnerName.Should().Be(mansion.OwnerName);

            RelationshipsInMansionResponse relationships = data.Relationships.Should().BeOfType<RelationshipsInMansionResponse>().Subject;

            relationships.Rooms.Should().NotBeNull();
            relationships.Rooms.Data.Should().BeNull();

            relationships.Staff.Should().NotBeNull();
            relationships.Staff.Data.Should().BeNull();
        });

        response.Data.OfType<DataInFamilyHomeResponse>().Should().ContainSingle(data => data.Id == familyHome.StringId).Subject.With(data =>
        {
            AttributesInFamilyHomeResponse attributes = data.Attributes.Should().BeOfType<AttributesInFamilyHomeResponse>().Subject;

            attributes.SurfaceInSquareMeters.Should().Be(familyHome.SurfaceInSquareMeters);
            attributes.NumberOfResidents.Should().Be(familyHome.NumberOfResidents);
            attributes.FloorCount.Should().Be(familyHome.FloorCount);

            RelationshipsInFamilyHomeResponse relationships = data.Relationships.Should().BeOfType<RelationshipsInFamilyHomeResponse>().Subject;

            relationships.Rooms.Should().NotBeNull();
            relationships.Rooms.Data.Should().BeNull();
        });
    }

    [Fact]
    public async Task Can_get_primary_resources_at_concrete_base_endpoint()
    {
        // Arrange
        Road road = _fakers.Road.GenerateOne();
        CyclePath cyclePath = _fakers.CyclePath.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Road>();
            dbContext.Roads.AddRange(road, cyclePath);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new NoOperationsInheritanceClient(requestAdapter);

        // Act
        RoadCollectionResponseDocument? response = await apiClient.Roads.GetAsync();

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().HaveCount(2);

        response.Data.Should().ContainSingle(data => data.Id == road.StringId).Subject.With(data =>
        {
            AttributesInRoadResponse? attributes = data.Attributes.Should().BeOfType<AttributesInRoadResponse>().Subject;

            ((decimal?)attributes.LengthInMeters).Should().BeApproximately(road.LengthInMeters);

            data.Relationships.Should().BeNull();
        });

        response.Data.OfType<DataInCyclePathResponse>().Should().ContainSingle(data => data.Id == cyclePath.StringId).Subject.With(data =>
        {
            AttributesInCyclePathResponse attributes = data.Attributes.Should().BeOfType<AttributesInCyclePathResponse>().Subject;

            ((decimal?)attributes.LengthInMeters).Should().BeApproximately(cyclePath.LengthInMeters);
            attributes.HasLaneForPedestrians.Should().Be(cyclePath.HasLaneForPedestrians);

            data.Relationships.Should().BeNull();
        });
    }

    [Fact]
    public async Task Can_get_primary_resources_at_concrete_derived_endpoint()
    {
        // Arrange
        FamilyHome familyHome1 = _fakers.FamilyHome.GenerateOne();
        FamilyHome familyHome2 = _fakers.FamilyHome.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Building>();
            dbContext.Buildings.AddRange(familyHome1, familyHome2);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new NoOperationsInheritanceClient(requestAdapter);

        // Act
        FamilyHomeCollectionResponseDocument? response = await apiClient.FamilyHomes.GetAsync();

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().HaveCount(2);

        response.Data.Should().ContainSingle(data => data.Id == familyHome1.StringId).Subject.With(data =>
        {
            AttributesInResidenceResponse attributes = data.Attributes.Should().BeOfType<AttributesInFamilyHomeResponse>().Subject;

            attributes.SurfaceInSquareMeters.Should().Be(familyHome1.SurfaceInSquareMeters);
            attributes.NumberOfResidents.Should().Be(familyHome1.NumberOfResidents);

            RelationshipsInResidenceResponse relationships = data.Relationships.Should().BeOfType<RelationshipsInFamilyHomeResponse>().Subject;

            relationships.Rooms.Should().NotBeNull();
            relationships.Rooms.Data.Should().BeNull();
        });

        response.Data.Should().ContainSingle(data => data.Id == familyHome2.StringId).Subject.With(data =>
        {
            AttributesInResidenceResponse attributes = data.Attributes.Should().BeOfType<AttributesInFamilyHomeResponse>().Subject;

            attributes.SurfaceInSquareMeters.Should().Be(familyHome2.SurfaceInSquareMeters);
            attributes.NumberOfResidents.Should().Be(familyHome2.NumberOfResidents);

            RelationshipsInResidenceResponse relationships = data.Relationships.Should().BeOfType<RelationshipsInFamilyHomeResponse>().Subject;

            relationships.Rooms.Should().NotBeNull();
            relationships.Rooms.Data.Should().BeNull();
        });
    }

    [Fact]
    public async Task Can_get_primary_resource_at_abstract_endpoint()
    {
        // Arrange
        Mansion mansion = _fakers.Mansion.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Buildings.Add(mansion);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new NoOperationsInheritanceClient(requestAdapter);

        // Act
        PrimaryBuildingResponseDocument? response = await apiClient.Buildings[mansion.StringId!].GetAsync();

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().BeOfType<DataInMansionResponse>();
        response.Data.Id.Should().Be(mansion.StringId);

        response.Data.Attributes.Should().BeOfType<AttributesInMansionResponse>().Subject.With(attributes =>
        {
            attributes.SurfaceInSquareMeters.Should().Be(mansion.SurfaceInSquareMeters);
            attributes.NumberOfResidents.Should().Be(mansion.NumberOfResidents);
            attributes.OwnerName.Should().Be(mansion.OwnerName);
        });

        response.Data.Relationships.Should().BeOfType<RelationshipsInMansionResponse>().Subject.With(relationships =>
        {
            relationships.Rooms.Should().NotBeNull();
            relationships.Rooms.Data.Should().BeNull();

            relationships.Staff.Should().NotBeNull();
            relationships.Staff.Data.Should().BeNull();
        });
    }

    [Fact]
    public async Task Can_get_primary_resource_at_concrete_base_endpoint()
    {
        // Arrange
        CyclePath cyclePath = _fakers.CyclePath.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Roads.Add(cyclePath);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new NoOperationsInheritanceClient(requestAdapter);

        // Act
        PrimaryRoadResponseDocument? response = await apiClient.Roads[cyclePath.StringId!].GetAsync();

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().BeOfType<DataInCyclePathResponse>();
        response.Data.Id.Should().Be(cyclePath.StringId);

        response.Data.Attributes.Should().BeOfType<AttributesInCyclePathResponse>().Subject.With(attributes =>
        {
            ((decimal?)attributes.LengthInMeters).Should().BeApproximately(cyclePath.LengthInMeters);
            attributes.HasLaneForPedestrians.Should().Be(cyclePath.HasLaneForPedestrians);
        });

        response.Data.Relationships.Should().BeNull();
    }

    [Fact]
    public async Task Can_get_primary_resource_at_concrete_derived_endpoint()
    {
        // Arrange
        FamilyHome familyHome = _fakers.FamilyHome.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Buildings.Add(familyHome);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new NoOperationsInheritanceClient(requestAdapter);

        // Act
        PrimaryFamilyHomeResponseDocument? response = await apiClient.FamilyHomes[familyHome.StringId!].GetAsync();

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().BeOfType<DataInFamilyHomeResponse>();
        response.Data.Id.Should().Be(familyHome.StringId);

        response.Data.Attributes.Should().BeOfType<AttributesInFamilyHomeResponse>().Subject.With(attributes =>
        {
            attributes.SurfaceInSquareMeters.Should().Be(familyHome.SurfaceInSquareMeters);
            attributes.NumberOfResidents.Should().Be(familyHome.NumberOfResidents);
        });

        response.Data.Relationships.Should().BeOfType<RelationshipsInFamilyHomeResponse>().Subject.With(relationships =>
        {
            relationships.Rooms.Should().NotBeNull();
            relationships.Rooms.Data.Should().BeNull();
        });
    }

    [Fact]
    public async Task Can_get_secondary_resource_at_abstract_base_endpoint()
    {
        // Arrange
        Bedroom bedroom = _fakers.Bedroom.GenerateOne();
        FamilyHome familyHome = _fakers.FamilyHome.GenerateOne();
        bedroom.Residence = familyHome;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Rooms.Add(bedroom);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new NoOperationsInheritanceClient(requestAdapter);

        // Act
        SecondaryResidenceResponseDocument? response = await apiClient.Rooms[bedroom.StringId!].Residence.GetAsync();

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().BeOfType<DataInFamilyHomeResponse>();
        response.Data.Id.Should().Be(bedroom.Residence.StringId);

        response.Data.Attributes.Should().BeOfType<AttributesInFamilyHomeResponse>().Subject.With(attributes =>
        {
            attributes.SurfaceInSquareMeters.Should().Be(familyHome.SurfaceInSquareMeters);
            attributes.NumberOfResidents.Should().Be(familyHome.NumberOfResidents);
            attributes.FloorCount.Should().Be(familyHome.FloorCount);
        });

        response.Data.Relationships.Should().BeOfType<RelationshipsInFamilyHomeResponse>().Subject.With(relationships =>
        {
            relationships.Rooms.Should().NotBeNull();
            relationships.Rooms.Data.Should().BeNull();
        });
    }

    [Fact]
    public async Task Can_get_secondary_resource_at_concrete_derived_endpoint()
    {
        // Arrange
        Bedroom bedroom = _fakers.Bedroom.GenerateOne();
        FamilyHome familyHome = _fakers.FamilyHome.GenerateOne();
        bedroom.Residence = familyHome;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Rooms.Add(bedroom);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new NoOperationsInheritanceClient(requestAdapter);

        // Act
        SecondaryResidenceResponseDocument? response = await apiClient.Bedrooms[bedroom.StringId!].Residence.GetAsync();

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().BeOfType<DataInFamilyHomeResponse>();
        response.Data.Id.Should().Be(bedroom.Residence.StringId);

        response.Data.Attributes.Should().BeOfType<AttributesInFamilyHomeResponse>().Subject.With(attributes =>
        {
            attributes.SurfaceInSquareMeters.Should().Be(familyHome.SurfaceInSquareMeters);
            attributes.NumberOfResidents.Should().Be(familyHome.NumberOfResidents);
            attributes.FloorCount.Should().Be(familyHome.FloorCount);
        });

        response.Data.Relationships.Should().BeOfType<RelationshipsInFamilyHomeResponse>().Subject.With(relationships =>
        {
            relationships.Rooms.Should().NotBeNull();
            relationships.Rooms.Data.Should().BeNull();
        });
    }

    [Fact]
    public async Task Can_get_secondary_resources_at_concrete_base_endpoint()
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
        var apiClient = new NoOperationsInheritanceClient(requestAdapter);

        // Act
        RoomCollectionResponseDocument? response = await apiClient.Residences[familyHome.StringId!].Rooms.GetAsync();

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().HaveCount(2);

        response.Data.OfType<DataInKitchenResponse>().Should().ContainSingle(data => data.Id == kitchen.StringId).Subject.With(data =>
        {
            AttributesInKitchenResponse attributes = data.Attributes.Should().BeOfType<AttributesInKitchenResponse>().Subject;

            attributes.SurfaceInSquareMeters.Should().Be(kitchen.SurfaceInSquareMeters);
            attributes.HasPantry.Should().Be(kitchen.HasPantry);

            RelationshipsInKitchenResponse relationships = data.Relationships.Should().BeOfType<RelationshipsInKitchenResponse>().Subject;

            relationships.Residence.Should().NotBeNull();
            relationships.Residence.Data.Should().BeNull();
        });

        response.Data.OfType<DataInBedroomResponse>().Should().ContainSingle(data => data.Id == bedroom.StringId).Subject.With(data =>
        {
            AttributesInBedroomResponse attributes = data.Attributes.Should().BeOfType<AttributesInBedroomResponse>().Subject;

            attributes.SurfaceInSquareMeters.Should().Be(bedroom.SurfaceInSquareMeters);
            attributes.BedCount.Should().Be(bedroom.BedCount);

            RelationshipsInBedroomResponse relationships = data.Relationships.Should().BeOfType<RelationshipsInBedroomResponse>().Subject;

            relationships.Residence.Should().NotBeNull();
            relationships.Residence.Data.Should().BeNull();
        });
    }

    [Fact]
    public async Task Can_get_secondary_resources_at_concrete_derived_endpoint()
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
        var apiClient = new NoOperationsInheritanceClient(requestAdapter);

        // Act
        RoomCollectionResponseDocument? response = await apiClient.Mansions[mansion.StringId!].Rooms.GetAsync();

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().HaveCount(2);

        response.Data.OfType<DataInBathroomResponse>().Should().ContainSingle(data => data.Id == bathroom.StringId).Subject.With(data =>
        {
            AttributesInBathroomResponse attributes = data.Attributes.Should().BeOfType<AttributesInBathroomResponse>().Subject;

            attributes.SurfaceInSquareMeters.Should().Be(bathroom.SurfaceInSquareMeters);
            attributes.HasBath.Should().Be(bathroom.HasBath);

            RelationshipsInBathroomResponse relationships = data.Relationships.Should().BeOfType<RelationshipsInBathroomResponse>().Subject;

            relationships.Residence.Should().NotBeNull();
            relationships.Residence.Data.Should().BeNull();
        });

        response.Data.OfType<DataInToiletResponse>().Should().ContainSingle(data => data.Id == toilet.StringId).Subject.With(data =>
        {
            AttributesInToiletResponse attributes = data.Attributes.Should().BeOfType<AttributesInToiletResponse>().Subject;

            attributes.SurfaceInSquareMeters.Should().Be(toilet.SurfaceInSquareMeters);
            attributes.HasSink.Should().Be(toilet.HasSink);

            RelationshipsInToiletResponse relationships = data.Relationships.Should().BeOfType<RelationshipsInToiletResponse>().Subject;

            relationships.Residence.Should().NotBeNull();
            relationships.Residence.Data.Should().BeNull();
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
                Type = RoadResourceType.Roads,
                Attributes = new AttributesInCreateRoadRequest
                {
                    LengthInMeters = newLengthInMeters
                }
            }
        };

        // Act
        PrimaryRoadResponseDocument? response = await apiClient.Roads.PostAsync(requestBody);

        // Assert
        response.Should().NotBeNull();

        DataInRoadResponse roadData = response.Data.Should().BeOfType<DataInRoadResponse>().Subject;

        AttributesInRoadResponse roadAttributes = roadData.Attributes.Should().BeOfType<AttributesInRoadResponse>().Subject;
        roadAttributes.LengthInMeters.Should().Be(newLengthInMeters);

        roadData.Relationships.Should().BeNull();

        long newRoadId = long.Parse(roadData.Id.Should().NotBeNull().And.Subject);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Road roadInDatabase = await dbContext.Roads.FirstWithIdAsync(newRoadId);

            roadInDatabase.LengthInMeters.Should().Be((decimal)newLengthInMeters);
        });
    }

    [Fact]
    public async Task Can_create_concrete_derived_resource_at_abstract_endpoint_with_relationships_and_includes()
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
                Type = BuildingResourceType.FamilyHomes,
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

        using IDisposable scope = _requestAdapterFactory.WithQueryString(new Dictionary<string, string?>
        {
            ["include"] = "rooms"
        });

        // Act
        PrimaryBuildingResponseDocument? response = await apiClient.Buildings.PostAsync(requestBody);

        // Assert
        response.Should().NotBeNull();

        DataInFamilyHomeResponse familyHomeData = response.Data.Should().BeOfType<DataInFamilyHomeResponse>().Subject;

        AttributesInFamilyHomeResponse familyHomeAttributes = familyHomeData.Attributes.Should().BeOfType<AttributesInFamilyHomeResponse>().Subject;
        familyHomeAttributes.SurfaceInSquareMeters.Should().Be(newFamilyHome.SurfaceInSquareMeters);
        familyHomeAttributes.NumberOfResidents.Should().Be(newFamilyHome.NumberOfResidents);
        familyHomeAttributes.FloorCount.Should().Be(newFamilyHome.FloorCount);

        RelationshipsInFamilyHomeResponse familyHomeRelationships = familyHomeData.Relationships.Should().BeOfType<RelationshipsInFamilyHomeResponse>().Subject;

        familyHomeRelationships.Rooms.RefShould().NotBeNull().And.Subject.Data.With(roomData =>
        {
            roomData.Should().HaveCount(3);
            roomData.OfType<BedroomIdentifierInResponse>().Should().ContainSingle(data => data.Id == existingBedroom1.StringId);
            roomData.OfType<BedroomIdentifierInResponse>().Should().ContainSingle(data => data.Id == existingBedroom2.StringId);
            roomData.OfType<LivingRoomIdentifierInResponse>().Should().ContainSingle(data => data.Id == existingLivingRoom.StringId);
        });

        long newFamilyHomeId = long.Parse(familyHomeData.Id.Should().NotBeNull().And.Subject);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            FamilyHome familyHomeInDatabase = await dbContext.FamilyHomes.Include(familyHome => familyHome.Rooms).FirstWithIdAsync(newFamilyHomeId);

            familyHomeInDatabase.SurfaceInSquareMeters.Should().Be(newFamilyHome.SurfaceInSquareMeters);
            familyHomeInDatabase.NumberOfResidents.Should().Be(newFamilyHome.NumberOfResidents);
            familyHomeInDatabase.FloorCount.Should().Be(newFamilyHome.FloorCount);

            familyHomeInDatabase.Rooms.Should().HaveCount(3);
            familyHomeInDatabase.Rooms.OfType<Bedroom>().Should().ContainSingle(bedroom => bedroom.Id == existingBedroom1.Id);
            familyHomeInDatabase.Rooms.OfType<Bedroom>().Should().ContainSingle(bedroom => bedroom.Id == existingBedroom2.Id);
            familyHomeInDatabase.Rooms.OfType<LivingRoom>().Should().ContainSingle(livingRoom => livingRoom.Id == existingLivingRoom.Id);
        });
    }

    [Fact]
    public async Task Can_create_concrete_derived_resource_at_concrete_base_endpoint()
    {
        // Arrange
        FamilyHome newFamilyHome = _fakers.FamilyHome.GenerateOne();

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new NoOperationsInheritanceClient(requestAdapter);

        CreateResidenceRequestDocument requestBody = new()
        {
            Data = new DataInCreateFamilyHomeRequest
            {
                Type = BuildingResourceType.FamilyHomes,
                Attributes = new AttributesInCreateFamilyHomeRequest
                {
                    SurfaceInSquareMeters = newFamilyHome.SurfaceInSquareMeters!.Value,
                    NumberOfResidents = newFamilyHome.NumberOfResidents!.Value,
                    FloorCount = newFamilyHome.FloorCount
                }
            }
        };

        // Act
        PrimaryResidenceResponseDocument? response = await apiClient.Residences.PostAsync(requestBody);

        // Assert
        response.Should().NotBeNull();

        DataInFamilyHomeResponse familyHomeData = response.Data.Should().BeOfType<DataInFamilyHomeResponse>().Subject;

        AttributesInFamilyHomeResponse familyHomeAttributes = familyHomeData.Attributes.Should().BeOfType<AttributesInFamilyHomeResponse>().Subject;
        familyHomeAttributes.SurfaceInSquareMeters.Should().Be(newFamilyHome.SurfaceInSquareMeters);
        familyHomeAttributes.NumberOfResidents.Should().Be(newFamilyHome.NumberOfResidents);
        familyHomeAttributes.FloorCount.Should().Be(newFamilyHome.FloorCount);

        RelationshipsInFamilyHomeResponse familyHomeRelationships = familyHomeData.Relationships.Should().BeOfType<RelationshipsInFamilyHomeResponse>().Subject;
        familyHomeRelationships.Rooms.Should().NotBeNull();

        long newFamilyHomeId = long.Parse(familyHomeData.Id.Should().NotBeNull().And.Subject);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            FamilyHome familyHomeInDatabase = await dbContext.FamilyHomes.Include(familyHome => familyHome.Rooms).FirstWithIdAsync(newFamilyHomeId);

            familyHomeInDatabase.SurfaceInSquareMeters.Should().Be(newFamilyHome.SurfaceInSquareMeters);
            familyHomeInDatabase.NumberOfResidents.Should().Be(newFamilyHome.NumberOfResidents);
            familyHomeInDatabase.FloorCount.Should().Be(newFamilyHome.FloorCount);

            familyHomeInDatabase.Rooms.Should().BeEmpty();
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
                Type = RoadResourceType.Roads,
                Id = existingRoad.StringId!,
                Attributes = new AttributesInUpdateRoadRequest
                {
                    LengthInMeters = newLengthInMeters
                }
            }
        };

        // Act
        PrimaryRoadResponseDocument? response = await apiClient.Roads[existingRoad.StringId!].PatchAsync(requestBody);

        // Assert
        response.Should().NotBeNull();

        DataInRoadResponse roadData = response.Data.Should().BeOfType<DataInRoadResponse>().Subject;

        AttributesInRoadResponse roadAttributes = roadData.Attributes.Should().BeOfType<AttributesInRoadResponse>().Subject;
        roadAttributes.LengthInMeters.Should().Be(newLengthInMeters);

        roadData.Relationships.Should().BeNull();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Road roadInDatabase = await dbContext.Roads.FirstWithIdAsync(existingRoad.Id);

            roadInDatabase.LengthInMeters.Should().Be((decimal)newLengthInMeters);
        });
    }

    [Fact]
    public async Task Can_update_concrete_derived_resource_at_abstract_endpoint_with_relationships_and_includes()
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
                Type = BuildingResourceType.FamilyHomes,
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

        using IDisposable scope = _requestAdapterFactory.WithQueryString(new Dictionary<string, string?>
        {
            ["include"] = "rooms"
        });

        // Act
        PrimaryBuildingResponseDocument? response = await apiClient.Buildings[existingFamilyHome.StringId!].PatchAsync(requestBody);

        // Assert
        response.Should().NotBeNull();

        DataInFamilyHomeResponse familyHomeData = response.Data.Should().BeOfType<DataInFamilyHomeResponse>().Subject;

        AttributesInFamilyHomeResponse familyHomeAttributes = familyHomeData.Attributes.Should().BeOfType<AttributesInFamilyHomeResponse>().Subject;
        familyHomeAttributes.SurfaceInSquareMeters.Should().Be(newFamilyHome.SurfaceInSquareMeters);
        familyHomeAttributes.NumberOfResidents.Should().Be(newFamilyHome.NumberOfResidents);
        familyHomeAttributes.FloorCount.Should().Be(newFamilyHome.FloorCount);

        RelationshipsInFamilyHomeResponse familyHomeRelationships = familyHomeData.Relationships.Should().BeOfType<RelationshipsInFamilyHomeResponse>().Subject;

        familyHomeRelationships.Rooms.RefShould().NotBeNull().And.Subject.Data.With(roomData =>
        {
            roomData.Should().HaveCount(3);
            roomData.OfType<BedroomIdentifierInResponse>().Should().ContainSingle(data => data.Id == existingBedroom1.StringId);
            roomData.OfType<BedroomIdentifierInResponse>().Should().ContainSingle(data => data.Id == existingBedroom2.StringId);
            roomData.OfType<LivingRoomIdentifierInResponse>().Should().ContainSingle(data => data.Id == existingLivingRoom.StringId);
        });

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            FamilyHome familyHomeInDatabase = await dbContext.FamilyHomes.Include(familyHome => familyHome.Rooms).FirstWithIdAsync(existingFamilyHome.Id);

            familyHomeInDatabase.SurfaceInSquareMeters.Should().Be(newFamilyHome.SurfaceInSquareMeters);
            familyHomeInDatabase.NumberOfResidents.Should().Be(newFamilyHome.NumberOfResidents);
            familyHomeInDatabase.FloorCount.Should().Be(newFamilyHome.FloorCount);

            familyHomeInDatabase.Rooms.Should().HaveCount(3);
            familyHomeInDatabase.Rooms.OfType<Bedroom>().Should().ContainSingle(bedroom => bedroom.Id == existingBedroom1.Id);
            familyHomeInDatabase.Rooms.OfType<Bedroom>().Should().ContainSingle(bedroom => bedroom.Id == existingBedroom2.Id);
            familyHomeInDatabase.Rooms.OfType<LivingRoom>().Should().ContainSingle(livingRoom => livingRoom.Id == existingLivingRoom.Id);
        });
    }

    [Fact]
    public async Task Can_update_concrete_derived_resource_at_concrete_base_endpoint()
    {
        // Arrange
        FamilyHome existingFamilyHome = _fakers.FamilyHome.GenerateOne();
        existingFamilyHome.Rooms.Add(_fakers.Kitchen.GenerateOne());

        FamilyHome newFamilyHome = _fakers.FamilyHome.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.FamilyHomes.Add(existingFamilyHome);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new NoOperationsInheritanceClient(requestAdapter);

        UpdateBuildingRequestDocument requestBody = new()
        {
            Data = new DataInUpdateFamilyHomeRequest
            {
                Type = BuildingResourceType.FamilyHomes,
                Id = existingFamilyHome.StringId!,
                Attributes = new AttributesInUpdateFamilyHomeRequest
                {
                    SurfaceInSquareMeters = newFamilyHome.SurfaceInSquareMeters!.Value,
                    NumberOfResidents = newFamilyHome.NumberOfResidents!.Value,
                    FloorCount = newFamilyHome.FloorCount
                }
            }
        };

        // Act
        PrimaryBuildingResponseDocument? response = await apiClient.Buildings[existingFamilyHome.StringId!].PatchAsync(requestBody);

        // Assert
        response.Should().NotBeNull();

        DataInFamilyHomeResponse familyHomeData = response.Data.Should().BeOfType<DataInFamilyHomeResponse>().Subject;

        AttributesInFamilyHomeResponse familyHomeAttributes = familyHomeData.Attributes.Should().BeOfType<AttributesInFamilyHomeResponse>().Subject;
        familyHomeAttributes.SurfaceInSquareMeters.Should().Be(newFamilyHome.SurfaceInSquareMeters);
        familyHomeAttributes.NumberOfResidents.Should().Be(newFamilyHome.NumberOfResidents);
        familyHomeAttributes.FloorCount.Should().Be(newFamilyHome.FloorCount);

        RelationshipsInFamilyHomeResponse familyHomeRelationships = familyHomeData.Relationships.Should().BeOfType<RelationshipsInFamilyHomeResponse>().Subject;
        familyHomeRelationships.Rooms.Should().NotBeNull();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            FamilyHome familyHomeInDatabase = await dbContext.FamilyHomes.Include(familyHome => familyHome.Rooms).FirstWithIdAsync(existingFamilyHome.Id);

            familyHomeInDatabase.SurfaceInSquareMeters.Should().Be(newFamilyHome.SurfaceInSquareMeters);
            familyHomeInDatabase.NumberOfResidents.Should().Be(newFamilyHome.NumberOfResidents);
            familyHomeInDatabase.FloorCount.Should().Be(newFamilyHome.FloorCount);

            familyHomeInDatabase.Rooms.Should().HaveCount(1);
            familyHomeInDatabase.Rooms.OfType<Kitchen>().Should().ContainSingle(kitchen => kitchen.Id == existingFamilyHome.Rooms.ElementAt(0).Id);
        });
    }

    [Fact]
    public async Task Can_delete_concrete_derived_resource_at_abstract_endpoint()
    {
        // Arrange
        Mansion existingMansion = _fakers.Mansion.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Mansions.Add(existingMansion);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new NoOperationsInheritanceClient(requestAdapter);

        // Act
        await apiClient.Buildings[existingMansion.StringId!].DeleteAsync();

        // Assert
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            FamilyHome? familyHomeInDatabase = await dbContext.FamilyHomes.FirstWithIdOrDefaultAsync(existingMansion.Id);

            familyHomeInDatabase.Should().BeNull();
        });
    }

    public void Dispose()
    {
        _requestAdapterFactory.Dispose();
    }
}
