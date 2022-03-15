using JetBrains.Annotations;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.TablePerHierarchy;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class TablePerHierarchyTests : ResourceInheritanceTests<TablePerHierarchyDbContext>
{
    public TablePerHierarchyTests(IntegrationTestContext<TestableStartup<TablePerHierarchyDbContext>, TablePerHierarchyDbContext> testContext)
        : base(testContext)
    {
    }
}
