using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Models;
using JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.TablePerHierarchy;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance;

public sealed class AtomicOperationTests : IClassFixture<IntegrationTestContext<TestableStartup<TablePerHierarchyDbContext>, TablePerHierarchyDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<TablePerHierarchyDbContext>, TablePerHierarchyDbContext> _testContext;
    private readonly ResourceInheritanceFakers _fakers = new();

    public AtomicOperationTests(IntegrationTestContext<TestableStartup<TablePerHierarchyDbContext>, TablePerHierarchyDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<OperationsController>();
    }

    [Fact]
    public async Task When_operation_is_enabled_on_base_type_it_is_implicitly_enabled_on_derived_types()
    {
        // Arrange
        AlwaysMovingTandem newMovingTandem = _fakers.AlwaysMovingTandem.GenerateOne();

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "alwaysMovingTandems",
                        attributes = new
                        {
                            weight = newMovingTandem.Weight,
                            requiresDriverLicense = newMovingTandem.RequiresDriverLicense,
                            gearCount = newMovingTandem.GearCount
                        }
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Results.ShouldHaveCount(1);

        responseDocument.Results[0].Data.SingleValue.ShouldNotBeNull().With(resource =>
        {
            resource.Type.Should().Be("alwaysMovingTandems");
            resource.Attributes.ShouldContainKey("weight").With(value => value.Should().Be(newMovingTandem.Weight));
            resource.Attributes.ShouldContainKey("requiresDriverLicense").With(value => value.Should().Be(newMovingTandem.RequiresDriverLicense));
            resource.Attributes.ShouldContainKey("gearCount").With(value => value.Should().Be(newMovingTandem.GearCount));
            resource.Relationships.Should().BeNull();
        });

        long newMovingTandemId = long.Parse(responseDocument.Results[0].Data.SingleValue!.Id.ShouldNotBeNull());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            AlwaysMovingTandem movingTandemInDatabase = await dbContext.AlwaysMovingTandems.FirstWithIdAsync(newMovingTandemId);

            movingTandemInDatabase.Weight.Should().Be(newMovingTandem.Weight);
            movingTandemInDatabase.RequiresDriverLicense.Should().Be(newMovingTandem.RequiresDriverLicense);
            movingTandemInDatabase.GearCount.Should().Be(newMovingTandem.GearCount);
        });
    }
}
