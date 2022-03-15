using JetBrains.Annotations;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.TablePerType;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class TablePerTypeTests : ResourceInheritanceTests<TablePerTypeDbContext>
{
    public TablePerTypeTests(IntegrationTestContext<TestableStartup<TablePerTypeDbContext>, TablePerTypeDbContext> testContext)
        : base(testContext)
    {
    }
}
