using System.Net;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Middleware;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.RestrictedControllers;

public sealed class EndpointFilterTests : IClassFixture<IntegrationTestContext<TestableStartup<RestrictionDbContext>, RestrictionDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<RestrictionDbContext>, RestrictionDbContext> _testContext;
    private readonly RestrictionFakers _fakers = new();

    public EndpointFilterTests(IntegrationTestContext<TestableStartup<RestrictionDbContext>, RestrictionDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<BedsController>();

        testContext.ConfigureServices(services => services.AddSingleton<IJsonApiEndpointFilter, NoRelationshipsAtBedJsonApiEndpointFilter>());
    }

    [Fact]
    public async Task Cannot_get_relationship()
    {
        // Arrange
        Bed bed = _fakers.Bed.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Beds.Add(bed);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/beds/{bed.StringId}/relationships/pillows";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    private sealed class NoRelationshipsAtBedJsonApiEndpointFilter : IJsonApiEndpointFilter
    {
        public bool IsEnabled(ResourceType resourceType, JsonApiEndpoints endpoint)
        {
            return !IsGetRelationshipAtBed(endpoint, resourceType);
        }

        private static bool IsGetRelationshipAtBed(JsonApiEndpoints endpoint, ResourceType resourceType)
        {
            bool isRelationshipEndpoint = endpoint is JsonApiEndpoints.GetRelationship or JsonApiEndpoints.PostRelationship or
                JsonApiEndpoints.PatchRelationship or JsonApiEndpoints.DeleteRelationship;

            return isRelationshipEndpoint && resourceType.ClrType == typeof(Bed);
        }
    }
}
