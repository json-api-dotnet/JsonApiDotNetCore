using JetBrains.Annotations;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.TablePerHierarchy;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class TablePerHierarchyWriteTests : ResourceInheritanceWriteTests<TablePerHierarchyDbContext>
{
    public TablePerHierarchyWriteTests(IntegrationTestContext<TestableStartup<TablePerHierarchyDbContext>, TablePerHierarchyDbContext> testContext)
        : base(testContext)
    {
    }
}
