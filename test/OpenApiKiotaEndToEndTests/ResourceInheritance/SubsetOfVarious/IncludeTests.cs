using FluentAssertions;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Kiota.Http.HttpClientLibrary;
using OpenApiKiotaEndToEndTests.ResourceInheritance.SubsetOfVarious.GeneratedCode;
using OpenApiKiotaEndToEndTests.ResourceInheritance.SubsetOfVarious.GeneratedCode.Models;
using OpenApiTests;
using OpenApiTests.ResourceInheritance;
using OpenApiTests.ResourceInheritance.Models;
using OpenApiTests.ResourceInheritance.SubsetOfVarious;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiKiotaEndToEndTests.ResourceInheritance.SubsetOfVarious;

public sealed class IncludeTests
    : IClassFixture<IntegrationTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext>>, IDisposable
{
    private readonly IntegrationTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> _testContext;
    private readonly TestableHttpClientRequestAdapterFactory _requestAdapterFactory;
    private readonly ResourceInheritanceFakers _fakers = new();

    public IncludeTests(IntegrationTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> testContext,
        ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _requestAdapterFactory = new TestableHttpClientRequestAdapterFactory(testOutputHelper);

        testContext.UseInheritanceControllers(false);

        testContext.ConfigureServices(services =>
        {
            services.AddSingleton<IJsonApiEndpointFilter, SubsetOfVariousEndpointFilter>();
            services.AddSingleton<IAtomicOperationFilter, SubsetOfVariousOperationFilter>();
        });
    }

    [Fact]
    public async Task Can_include_in_primary_resources()
    {
        // Arrange
        District district = _fakers.District.GenerateOne();

        FamilyHome familyHome = _fakers.FamilyHome.GenerateOne();
        familyHome.Rooms.Add(_fakers.LivingRoom.GenerateOne());
        familyHome.Rooms.Add(_fakers.Bedroom.GenerateOne());
        district.Buildings.Add(familyHome);

        Mansion mansion = _fakers.Mansion.GenerateOne();
        mansion.Rooms.Add(_fakers.Kitchen);
        mansion.Rooms.Add(_fakers.Bathroom);
        mansion.Rooms.Add(_fakers.Toilet);
        district.Buildings.Add(mansion);

        Residence residence = _fakers.Residence.GenerateOne();
        residence.Rooms.Add(_fakers.Bedroom.GenerateOne());
        district.Buildings.Add(residence);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Districts.Add(district);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new SubsetOfVariousInheritanceClient(requestAdapter);

        var queryString = new Dictionary<string, string?>
        {
            ["include"] = "buildings.rooms"
        };

        using (_requestAdapterFactory.WithQueryString(queryString))
        {
            // Act
            DistrictCollectionResponseDocument? response = await apiClient.Districts.GetAsync();

            // Assert
            response.ShouldNotBeNull();
            response.Data.ShouldHaveCount(1);
            response.Data.ElementAt(0).Id.Should().Be(district.StringId);

            response.Included.ShouldHaveCount(9);

            string familyHomeLivingRoomId = familyHome.Rooms.OfType<LivingRoom>().Single().StringId!;
            string familyRoomBedroomId = familyHome.Rooms.OfType<Bedroom>().Single().StringId!;
            string mansionKitchenId = mansion.Rooms.OfType<Kitchen>().Single().StringId!;
            string mansionBathroomId = mansion.Rooms.OfType<Bathroom>().Single().StringId!;
            string mansionToiletId = mansion.Rooms.OfType<Toilet>().Single().StringId!;
            string residenceBedroomId = residence.Rooms.OfType<Bedroom>().Single().StringId!;

            response.Included.OfType<FamilyHomeDataInResponse>().Should().ContainSingle(familyHomeData => familyHomeData.Id == familyHome.StringId).Subject
                .With(familyHomeData =>
                {
                    FamilyHomeAttributesInResponse attributes = familyHomeData.Attributes.Should().BeOfType<FamilyHomeAttributesInResponse>().Subject;

                    attributes.SurfaceInSquareMeters.Should().Be(familyHome.SurfaceInSquareMeters);
                    attributes.NumberOfResidents.Should().Be(familyHome.NumberOfResidents);
                    attributes.FloorCount.Should().Be(familyHome.FloorCount);

                    FamilyHomeRelationshipsInResponse relationships =
                        familyHomeData.Relationships.Should().BeOfType<FamilyHomeRelationshipsInResponse>().Subject;

                    relationships.Rooms.ShouldNotBeNull();
                    relationships.Rooms.Data.ShouldHaveCount(2);
                    relationships.Rooms.Data.OfType<LivingRoomIdentifierInResponse>().Should().ContainSingle(room => room.Id == familyHomeLivingRoomId);
                    relationships.Rooms.Data.OfType<BedroomIdentifierInResponse>().Should().ContainSingle(bedroom => bedroom.Id == familyRoomBedroomId);
                });

            response.Included.OfType<MansionDataInResponse>().Should().ContainSingle(mansionData => mansionData.Id == mansion.StringId).Subject.With(
                mansionData =>
                {
                    MansionAttributesInResponse attributes = mansionData.Attributes.Should().BeOfType<MansionAttributesInResponse>().Subject;

                    attributes.SurfaceInSquareMeters.Should().Be(mansion.SurfaceInSquareMeters);
                    attributes.NumberOfResidents.Should().Be(mansion.NumberOfResidents);
                    attributes.OwnerName.Should().Be(mansion.OwnerName);

                    MansionRelationshipsInResponse relationships = mansionData.Relationships.Should().BeOfType<MansionRelationshipsInResponse>().Subject;

                    relationships.Rooms.ShouldNotBeNull();
                    relationships.Rooms.Data.ShouldHaveCount(3);
                    relationships.Rooms.Data.OfType<KitchenIdentifierInResponse>().Should().ContainSingle(kitchen => kitchen.Id == mansionKitchenId);
                    relationships.Rooms.Data.OfType<BathroomIdentifierInResponse>().Should().ContainSingle(bathroom => bathroom.Id == mansionBathroomId);
                    relationships.Rooms.Data.OfType<ToiletIdentifierInResponse>().Should().ContainSingle(toilet => toilet.Id == mansionToiletId);
                });

            response.Included.OfType<ResidenceDataInResponse>().Should().ContainSingle(residenceData => residenceData.Id == residence.StringId).Subject.With(
                residenceData =>
                {
                    ResidenceAttributesInResponse attributes = residenceData.Attributes.Should().BeOfType<ResidenceAttributesInResponse>().Subject;

                    attributes.SurfaceInSquareMeters.Should().Be(residence.SurfaceInSquareMeters);
                    attributes.NumberOfResidents.Should().Be(residence.NumberOfResidents);

                    ResidenceRelationshipsInResponse relationships = residenceData.Relationships.Should().BeOfType<ResidenceRelationshipsInResponse>().Subject;

                    relationships.Rooms.ShouldNotBeNull();
                    relationships.Rooms.Data.ShouldHaveCount(1);
                    relationships.Rooms.Data.OfType<BedroomIdentifierInResponse>().Should().ContainSingle(bedroom => bedroom.Id == residenceBedroomId);
                });

            response.Included.OfType<LivingRoomDataInResponse>().Should().ContainSingle(livingRoom => livingRoom.Id == familyHomeLivingRoomId);
            response.Included.OfType<BedroomDataInResponse>().Should().ContainSingle(livingRoom => livingRoom.Id == familyRoomBedroomId);
            response.Included.OfType<KitchenDataInResponse>().Should().ContainSingle(livingRoom => livingRoom.Id == mansionKitchenId);
            response.Included.OfType<BathroomDataInResponse>().Should().ContainSingle(livingRoom => livingRoom.Id == mansionBathroomId);
            response.Included.OfType<ToiletDataInResponse>().Should().ContainSingle(livingRoom => livingRoom.Id == mansionToiletId);
            response.Included.OfType<BedroomDataInResponse>().Should().ContainSingle(livingRoom => livingRoom.Id == residenceBedroomId);
        }
    }

    public void Dispose()
    {
        _requestAdapterFactory.Dispose();
    }
}
