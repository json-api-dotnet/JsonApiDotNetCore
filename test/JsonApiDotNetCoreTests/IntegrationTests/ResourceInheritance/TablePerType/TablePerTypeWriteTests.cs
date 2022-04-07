using JetBrains.Annotations;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.TablePerType;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class TablePerTypeWriteTests : ResourceInheritanceWriteTests<TablePerTypeDbContext>
{
    public TablePerTypeWriteTests(IntegrationTestContext<TestableStartup<TablePerTypeDbContext>, TablePerTypeDbContext> testContext)
        : base(testContext)
    {
    }
}
