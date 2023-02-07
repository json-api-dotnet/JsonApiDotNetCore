using JetBrains.Annotations;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.TablePerConcreteType;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class TablePerConcreteTypeReadTests : ResourceInheritanceReadTests<TablePerConcreteTypeDbContext>
{
    public TablePerConcreteTypeReadTests(IntegrationTestContext<TestableStartup<TablePerConcreteTypeDbContext>, TablePerConcreteTypeDbContext> testContext)
        : base(testContext)
    {
    }
}
