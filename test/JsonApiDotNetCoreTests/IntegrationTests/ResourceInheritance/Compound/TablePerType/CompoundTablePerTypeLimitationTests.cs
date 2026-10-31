using FluentAssertions;
using JetBrains.Annotations;
using JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Compound.Models;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Compound.TablePerType;

/// <summary>
/// The read and write tests of the other inheritance mappings cannot run against this mapping. This test detects when the EF Core limitation is fixed,
/// in which case it fails and the test classes should be derived from <see cref="CompoundInheritanceReadTests{TDbContext}" /> and
/// <see cref="CompoundInheritanceWriteTests{TDbContext}" /> (as the TablePerHierarchy variants do), with the TPT mapping. xUnit v2 offers no way to skip
/// a derived test class as a whole.
/// </summary>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class CompoundTablePerTypeLimitationTests(
    IntegrationTestContext<TestableStartup<CompoundTablePerTypeDbContext>, CompoundTablePerTypeDbContext> testContext)
    : IClassFixture<IntegrationTestContext<TestableStartup<CompoundTablePerTypeDbContext>, CompoundTablePerTypeDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<CompoundTablePerTypeDbContext>, CompoundTablePerTypeDbContext> _testContext = testContext;
    private readonly CompoundInheritanceFakers _fakers = new();

    [Fact]
    public async Task EF_Core_10_cannot_use_json_complex_properties_with_table_per_type_mapping()
    {
        // Arrange
        Apartment apartment = _fakers.Apartment.GenerateOne();

        // Act
        Func<Task> action = async () => await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Listings.Add(apartment);
            await dbContext.SaveChangesAsync();
        });

        // Assert
        await action.Should().ThrowAsync<ArgumentException>().WithMessage("An item with the same key has already been added*");
    }
}
