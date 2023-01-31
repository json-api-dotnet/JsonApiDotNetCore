using JetBrains.Annotations;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.TablePerConcreteType;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class TablePerConcreteTypeWriteTests : ResourceInheritanceWriteTests<TablePerConcreteTypeDbContext>
{
    public TablePerConcreteTypeWriteTests(IntegrationTestContext<TestableStartup<TablePerConcreteTypeDbContext>, TablePerConcreteTypeDbContext> testContext)
        : base(testContext)
    {
    }
}
