using JetBrains.Annotations;
using JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Compound.Models;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Compound.TablePerConcreteType;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class CompoundTablePerConcreteTypeInsertTests(
    IntegrationTestContext<TestableStartup<CompoundTablePerConcreteTypeDbContext>, CompoundTablePerConcreteTypeDbContext> testContext)
    : IClassFixture<IntegrationTestContext<TestableStartup<CompoundTablePerConcreteTypeDbContext>, CompoundTablePerConcreteTypeDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<CompoundTablePerConcreteTypeDbContext>, CompoundTablePerConcreteTypeDbContext> _testContext =
        testContext;

    private readonly CompoundInheritanceFakers _fakers = new();

    [Fact]
    public async Task Can_insert_derived_type_when_abstract_base_declares_json_complex_properties()
    {
        // Arrange
        Apartment apartment = _fakers.Apartment.GenerateOne();

        // Act
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Listings.Add(apartment);
            await dbContext.SaveChangesAsync();
        });

        // Assert
        apartment.Id.Should().NotBe(0);
    }
}
