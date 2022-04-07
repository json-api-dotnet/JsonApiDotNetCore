using JetBrains.Annotations;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.TablePerType;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class TablePerTypeReadTests : ResourceInheritanceReadTests<TablePerTypeDbContext>
{
    public TablePerTypeReadTests(IntegrationTestContext<TestableStartup<TablePerTypeDbContext>, TablePerTypeDbContext> testContext)
        : base(testContext)
    {
    }
}
