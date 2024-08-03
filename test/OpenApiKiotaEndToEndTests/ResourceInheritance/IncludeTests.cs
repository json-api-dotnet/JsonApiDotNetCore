using FluentAssertions;
using Microsoft.Kiota.Http.HttpClientLibrary;
using OpenApiKiotaEndToEndTests.ResourceInheritance.GeneratedCode;
using OpenApiKiotaEndToEndTests.ResourceInheritance.GeneratedCode.Models;
using OpenApiTests;
using OpenApiTests.ResourceInheritance;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiKiotaEndToEndTests.ResourceInheritance;

public sealed class IncludeTests : IClassFixture<IntegrationTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext>>
{
    private readonly IntegrationTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> _testContext;
    private readonly TestableHttpClientRequestAdapterFactory _requestAdapterFactory;
    private readonly ResourceInheritanceFakers _fakers = new();

    public IncludeTests(IntegrationTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> testContext,
        ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _requestAdapterFactory = new TestableHttpClientRequestAdapterFactory(testOutputHelper);

        testContext.UseController<DistrictsController>();
        testContext.UseController<BuildingsController>();
        testContext.UseController<ResidencesController>();
        testContext.UseController<FamilyHomesController>();
        testContext.UseController<MansionsController>();
    }

    [Fact]
    public async Task Can_include_in_primary_resources()
    {
        // Arrange
        District district = _fakers.District.Generate();

        FamilyHome familyHome = _fakers.FamilyHome.Generate();
        district.Buildings.Add(familyHome);

        Mansion mansion = _fakers.Mansion.Generate();
        district.Buildings.Add(mansion);

        Residence residence = _fakers.Residence.Generate();
        district.Buildings.Add(residence);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Districts.Add(district);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new ResourceInheritanceClient(requestAdapter);

        var queryString = new Dictionary<string, string?>
        {
            ["include"] = "buildings"
        };

        using (_requestAdapterFactory.WithQueryString(queryString))
        {
            // Act
            DistrictCollectionResponseDocument? response = await apiClient.Districts.GetAsync();

            // Assert
            response.ShouldNotBeNull();
            response.Data.ShouldHaveCount(1);
            response.Data.ElementAt(0).Id.Should().Be(district.StringId);

            response.Included.ShouldHaveCount(3);

            response.Included.Should().ContainSingle(include => include is FamilyHomeDataInResponse && ((FamilyHomeDataInResponse)include).Id == familyHome.StringId);
            response.Included.Should().ContainSingle(include => include is MansionDataInResponse && ((MansionDataInResponse)include).Id == mansion.StringId);
            response.Included.Should().ContainSingle(include => include is ResidenceDataInResponse && ((ResidenceDataInResponse)include).Id == residence.StringId);
        }
    }
}
