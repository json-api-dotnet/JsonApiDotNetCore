using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.CustomRoutes;

public sealed class CustomRouteTests : IClassFixture<IntegrationTestContext<TestableStartup<CustomRouteDbContext>, CustomRouteDbContext>>
{
    private const string HostPrefix = "http://localhost";

    private readonly IntegrationTestContext<TestableStartup<CustomRouteDbContext>, CustomRouteDbContext> _testContext;
    private readonly CustomRouteFakers _fakers = new();

    public CustomRouteTests(IntegrationTestContext<TestableStartup<CustomRouteDbContext>, CustomRouteDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<CiviliansController>();
        testContext.UseController<TownsController>();
    }

    [Fact]
    public async Task Can_get_resource_at_custom_route()
    {
        // Arrange
        Town town = _fakers.Town.GenerateOne();
        town.Civilians = _fakers.Civilian.GenerateSet(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Towns.Add(town);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/world-api/civilization/popular/towns/{town.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("towns");
        responseDocument.Data.SingleValue.Id.Should().Be(town.StringId);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("name").WhoseValue.Should().Be(town.Name);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("latitude").WhoseValue.Should().Be(town.Latitude);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("longitude").WhoseValue.Should().Be(town.Longitude);

        responseDocument.Data.SingleValue.Relationships.Should().ContainKey("civilians").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();
            value.Links.Should().NotBeNull();
            value.Links.Self.Should().Be($"{HostPrefix}{route}/relationships/civilians");
            value.Links.Related.Should().Be($"{HostPrefix}{route}/civilians");
        });

        responseDocument.Data.SingleValue.Links.Should().NotBeNull();
        responseDocument.Data.SingleValue.Links.Self.Should().Be($"{HostPrefix}{route}");
        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
    }

    [Fact]
    public async Task Can_get_resources_at_custom_action_method()
    {
        // Arrange
        List<Town> towns = _fakers.Town.GenerateList(7);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Town>();
            dbContext.Towns.AddRange(towns);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/world-api/civilization/popular/towns/largest-5";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(5);
        responseDocument.Data.ManyValue.Should().OnlyContain(resource => resource.Type == "towns");
        responseDocument.Data.ManyValue.Should().OnlyContain(resource => resource.Attributes != null && resource.Attributes.Count > 0);
        responseDocument.Data.ManyValue.Should().OnlyContain(resource => resource.Relationships != null && resource.Relationships.Count > 0);
    }
}
