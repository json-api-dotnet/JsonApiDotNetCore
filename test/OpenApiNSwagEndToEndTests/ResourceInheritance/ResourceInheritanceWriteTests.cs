using FluentAssertions;
using JsonApiDotNetCore.OpenApi.Client.NSwag;
using Microsoft.EntityFrameworkCore;
using OpenApiNSwagEndToEndTests.ResourceInheritance.GeneratedCode;
using OpenApiTests;
using OpenApiTests.ResourceInheritance;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiNSwagEndToEndTests.ResourceInheritance;

public sealed class ResourceInheritanceWriteTests
    : IClassFixture<IntegrationTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext>>, IDisposable
{
    private readonly IntegrationTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> _testContext;
    private readonly XUnitLogHttpMessageHandler _logHttpMessageHandler;
    private readonly ResourceInheritanceFakers _fakers = new();

    // TODO: Verify that PATCH endpoints on abstract resource types aren't generated.

    public ResourceInheritanceWriteTests(IntegrationTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> testContext,
        ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _logHttpMessageHandler = new XUnitLogHttpMessageHandler(testOutputHelper);

        testContext.UseController<DistrictsController>();
        testContext.UseController<BuildingsController>();
    }

    // TODO: Add relationships into the mix.
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

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new ResourceInheritanceClient(httpClient);

        CreateBuildingRequestDocument requestBody = new()
        {
            Data = new DataInCreateFamilyHomeRequest
            {
                Attributes = new AttributesInCreateFamilyHomeRequest
                {
                    // TODO: Why are the first two properties generated as non-nullable?
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
                                // TODO: Can we eliminate the need to set Type?
                                Type = RoomResourceType.Bedrooms,
                                Id = existingBedroom1.StringId!
                            },
                            new RoomIdentifierInRequest
                            {
                                Type = RoomResourceType.Bedrooms,
                                Id = existingBedroom2.StringId!
                            },
                            new RoomIdentifierInRequest
                            {
                                Type = RoomResourceType.LivingRooms,
                                Id = existingLivingRoom.StringId!
                            }
                        ]
                    }
                }
            }
        };

        // Act
        BuildingPrimaryResponseDocument? response = await ApiResponse.TranslateAsync(async () => await apiClient.PostBuildingAsync(requestBody));

        // Assert
        response.ShouldNotBeNull();

        FamilyHomeDataInResponse? familyHomeData = response.Data.Should().BeOfType<FamilyHomeDataInResponse>().Subject;
        FamilyHomeAttributesInResponse? familyHomeAttributes = familyHomeData.Attributes.Should().BeOfType<FamilyHomeAttributesInResponse>().Subject;

        familyHomeAttributes.SurfaceInSquareMeters.Should().Be(newFamilyHome.SurfaceInSquareMeters);
        familyHomeAttributes.NumberOfResidents.Should().Be(newFamilyHome.NumberOfResidents);
        familyHomeAttributes.FloorCount.Should().Be(newFamilyHome.FloorCount);
        // TODO: Why is the "rooms" relationship { links } not returned? Looks like a bug...
        familyHomeData.Relationships.Should().BeNull();

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

    public void Dispose()
    {
        _logHttpMessageHandler.Dispose();
    }
}
