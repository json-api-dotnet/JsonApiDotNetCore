using FluentAssertions;
using JsonApiDotNetCore.OpenApi.Client.NSwag;
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
        FamilyHome newFamilyHome = _fakers.FamilyHome.GenerateOne();

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
                }
            }
        };

        // Act
        BuildingPrimaryResponseDocument? response = await ApiResponse.TranslateAsync(async () => await apiClient.PostBuildingAsync(requestBody));

        // Assert
        response.ShouldNotBeNull();

        var familyHomeData = response.Data.Should().BeOfType<FamilyHomeDataInResponse>().Subject;
        var familyHomeAttributes = familyHomeData.Attributes.Should().BeOfType<FamilyHomeAttributesInResponse>().Subject;

        familyHomeAttributes.SurfaceInSquareMeters.Should().Be(newFamilyHome.SurfaceInSquareMeters);
        familyHomeAttributes.NumberOfResidents.Should().Be(newFamilyHome.NumberOfResidents);
        familyHomeAttributes.FloorCount.Should().Be(newFamilyHome.FloorCount);
        // TODO: Assert on relationships.
        //familyHomeData.Relationships.ShouldNotBeNull();

        long newFamilyHomeId = long.Parse(familyHomeData.Id.ShouldNotBeNull());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:wrap_after_property_in_chained_method_calls true

            FamilyHome familyHomeInDatabase = await dbContext.FamilyHomes
                //.Include(car => car.Manufacturer)
                //.Include(car => car.Wheels)
                //.Include(car => car.Engine)
                //.Include(car => car.NavigationSystem)
                //.Include(car => car.Features)
                .FirstWithIdAsync(newFamilyHomeId);

            // @formatter:wrap_after_property_in_chained_method_calls restore
            // @formatter:wrap_chained_method_calls restore

            familyHomeInDatabase.SurfaceInSquareMeters.Should().Be(newFamilyHome.SurfaceInSquareMeters);
            familyHomeInDatabase.NumberOfResidents.Should().Be(newFamilyHome.NumberOfResidents);
            familyHomeInDatabase.FloorCount.Should().Be(newFamilyHome.FloorCount);
            // TODO: Assert on relationships.
        });
    }

    public void Dispose()
    {
        _logHttpMessageHandler.Dispose();
    }
}
