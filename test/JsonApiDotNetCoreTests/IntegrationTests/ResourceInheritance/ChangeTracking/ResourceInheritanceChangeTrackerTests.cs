using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Models;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.ChangeTracking;

public sealed class ResourceInheritanceChangeTrackerTests
    : IClassFixture<IntegrationTestContext<TestableStartup<ChangeTrackingDbContext>, ChangeTrackingDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<ChangeTrackingDbContext>, ChangeTrackingDbContext> _testContext;
    private readonly ResourceInheritanceFakers _fakers = new();

    public ResourceInheritanceChangeTrackerTests(IntegrationTestContext<TestableStartup<ChangeTrackingDbContext>, ChangeTrackingDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<VehiclesController>();
    }

    [Fact]
    public async Task Can_detect_side_effects_in_derived_type_at_abstract_endpoint()
    {
        // Arrange
        AlwaysMovingTandem existingMovingTandem = _fakers.AlwaysMovingTandem.Generate();

        int newGearCount = _fakers.AlwaysMovingTandem.Generate().GearCount;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Vehicles.Add(existingMovingTandem);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "bikes",
                id = existingMovingTandem.StringId,
                attributes = new
                {
                    gearCount = newGearCount
                }
            }
        };

        string route = $"/vehicles/{existingMovingTandem.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("alwaysMovingTandems");
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("locationToken").With(value => value.Should().NotBe(existingMovingTandem.LocationToken));
    }
}
