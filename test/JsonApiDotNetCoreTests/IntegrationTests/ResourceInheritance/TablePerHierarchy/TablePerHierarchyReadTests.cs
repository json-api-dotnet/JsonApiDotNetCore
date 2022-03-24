using JetBrains.Annotations;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.TablePerHierarchy;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class TablePerHierarchyReadTests : ResourceInheritanceReadTests<TablePerHierarchyDbContext>
{
    public TablePerHierarchyReadTests(IntegrationTestContext<TestableStartup<TablePerHierarchyDbContext>, TablePerHierarchyDbContext> testContext)
        : base(testContext)
    {
    }
}
