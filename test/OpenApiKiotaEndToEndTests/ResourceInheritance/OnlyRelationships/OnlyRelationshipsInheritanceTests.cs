using FluentAssertions;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Middleware;
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

public sealed class OnlyRelationshipsInheritanceTests
    : IClassFixture<IntegrationTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext>>, IDisposable
{
    private readonly IntegrationTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> _testContext;
    private readonly TestableHttpClientRequestAdapterFactory _requestAdapterFactory;
    private readonly ResourceInheritanceFakers _fakers = new();

    public OnlyRelationshipsInheritanceTests(IntegrationTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> testContext,
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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new OnlyRelationshipsInheritanceClient(requestAdapter);

        // Act
        ResidenceIdentifierResponseDocument? response = await apiClient.Rooms[bedroom.StringId].Relationships.Residence.GetAsync();

        // Assert
        response.ShouldNotBeNull();
        response.Data.ShouldNotBeNull();
        response.Data.Should().BeOfType<FamilyHomeIdentifierInResponse>();
        response.Data.Id.Should().Be(bedroom.Residence.StringId);
    }

    public void Dispose()
    {
        _requestAdapterFactory.Dispose();
    }
}
