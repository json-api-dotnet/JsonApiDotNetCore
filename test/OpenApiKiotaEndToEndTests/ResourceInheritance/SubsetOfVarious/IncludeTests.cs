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
        mansion.Rooms.Add(_fakers.Kitchen.GenerateOne());
        mansion.Rooms.Add(_fakers.Bathroom.GenerateOne());
        mansion.Rooms.Add(_fakers.Toilet.GenerateOne());
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

        using IDisposable scope = _requestAdapterFactory.WithQueryString(new Dictionary<string, string?>
        {
            ["include"] = "buildings.rooms"
        });

        // Act
        DistrictCollectionResponseDocument? response = await apiClient.Districts.GetAsync();

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().HaveCount(1);
        response.Data.ElementAt(0).Id.Should().Be(district.StringId);

        response.Included.Should().HaveCount(9);

        string familyHomeLivingRoomId = familyHome.Rooms.OfType<LivingRoom>().Single().StringId!;
        string familyRoomBedroomId = familyHome.Rooms.OfType<Bedroom>().Single().StringId!;
        string mansionKitchenId = mansion.Rooms.OfType<Kitchen>().Single().StringId!;
        string mansionBathroomId = mansion.Rooms.OfType<Bathroom>().Single().StringId!;
        string mansionToiletId = mansion.Rooms.OfType<Toilet>().Single().StringId!;
        string residenceBedroomId = residence.Rooms.OfType<Bedroom>().Single().StringId!;

        response.Included.OfType<DataInFamilyHomeResponse>().Should().ContainSingle(include => include.Id == familyHome.StringId).Subject.With(include =>
        {
            AttributesInFamilyHomeResponse attributes = include.Attributes.Should().BeOfType<AttributesInFamilyHomeResponse>().Subject;

            attributes.SurfaceInSquareMeters.Should().Be(familyHome.SurfaceInSquareMeters);
            attributes.NumberOfResidents.Should().Be(familyHome.NumberOfResidents);
            attributes.FloorCount.Should().Be(familyHome.FloorCount);

            RelationshipsInFamilyHomeResponse relationships = include.Relationships.Should().BeOfType<RelationshipsInFamilyHomeResponse>().Subject;

            relationships.Rooms.Should().NotBeNull();
            relationships.Rooms.Data.Should().HaveCount(2);
            relationships.Rooms.Data.OfType<LivingRoomIdentifierInResponse>().Should().ContainSingle(data => data.Id == familyHomeLivingRoomId);
            relationships.Rooms.Data.OfType<BedroomIdentifierInResponse>().Should().ContainSingle(data => data.Id == familyRoomBedroomId);
        });

        response.Included.OfType<DataInMansionResponse>().Should().ContainSingle(include => include.Id == mansion.StringId).Subject.With(include =>
        {
            AttributesInMansionResponse attributes = include.Attributes.Should().BeOfType<AttributesInMansionResponse>().Subject;

            attributes.SurfaceInSquareMeters.Should().Be(mansion.SurfaceInSquareMeters);
            attributes.NumberOfResidents.Should().Be(mansion.NumberOfResidents);
            attributes.OwnerName.Should().Be(mansion.OwnerName);

            RelationshipsInMansionResponse relationships = include.Relationships.Should().BeOfType<RelationshipsInMansionResponse>().Subject;

            relationships.Rooms.Should().NotBeNull();
            relationships.Rooms.Data.Should().HaveCount(3);
            relationships.Rooms.Data.OfType<KitchenIdentifierInResponse>().Should().ContainSingle(data => data.Id == mansionKitchenId);
            relationships.Rooms.Data.OfType<BathroomIdentifierInResponse>().Should().ContainSingle(data => data.Id == mansionBathroomId);
            relationships.Rooms.Data.OfType<ToiletIdentifierInResponse>().Should().ContainSingle(data => data.Id == mansionToiletId);
        });

        response.Included.OfType<DataInResidenceResponse>().Should().ContainSingle(include => include.Id == residence.StringId).Subject.With(include =>
        {
            AttributesInResidenceResponse attributes = include.Attributes.Should().BeOfType<AttributesInResidenceResponse>().Subject;

            attributes.SurfaceInSquareMeters.Should().Be(residence.SurfaceInSquareMeters);
            attributes.NumberOfResidents.Should().Be(residence.NumberOfResidents);

            RelationshipsInResidenceResponse relationships = include.Relationships.Should().BeOfType<RelationshipsInResidenceResponse>().Subject;

            relationships.Rooms.Should().NotBeNull();
            relationships.Rooms.Data.Should().HaveCount(1);
            relationships.Rooms.Data.OfType<BedroomIdentifierInResponse>().Should().ContainSingle(data => data.Id == residenceBedroomId);
        });

        response.Included.OfType<DataInLivingRoomResponse>().Should().ContainSingle(include => include.Id == familyHomeLivingRoomId);
        response.Included.OfType<DataInBedroomResponse>().Should().ContainSingle(include => include.Id == familyRoomBedroomId);
        response.Included.OfType<DataInKitchenResponse>().Should().ContainSingle(include => include.Id == mansionKitchenId);
        response.Included.OfType<DataInBathroomResponse>().Should().ContainSingle(include => include.Id == mansionBathroomId);
        response.Included.OfType<DataInToiletResponse>().Should().ContainSingle(include => include.Id == mansionToiletId);
        response.Included.OfType<DataInBedroomResponse>().Should().ContainSingle(include => include.Id == residenceBedroomId);
    }

    public void Dispose()
    {
        _requestAdapterFactory.Dispose();
    }
}
