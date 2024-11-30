using FluentAssertions;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.OpenApi.Client.NSwag;
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

        testContext.UseController<DistrictsController>();

        testContext.UseController<BuildingsController>();
        testContext.UseController<ResidencesController>();
        testContext.UseController<FamilyHomesController>();
        testContext.UseController<MansionsController>();

        testContext.UseController<RoomsController>();
        testContext.UseController<KitchensController>();
        testContext.UseController<BedroomsController>();
        testContext.UseController<BathroomsController>();
        testContext.UseController<LivingRoomsController>();
        testContext.UseController<ToiletsController>();

        testContext.ConfigureServices(services => services.AddSingleton<IJsonApiEndpointFilter, OnlyRelationshipsEndpointFilter>());
    }

    [Fact]
    public async Task Test3()
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

    public void Dispose()
    {
        _logHttpMessageHandler.Dispose();
    }
}
