using JetBrains.Annotations;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.TablePerConcreteType;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class TablePerConcreteTypeWriteTests(
    IntegrationTestContext<TestableStartup<TablePerConcreteTypeDbContext>, TablePerConcreteTypeDbContext> testContext)
    : ResourceInheritanceWriteTests<TablePerConcreteTypeDbContext>(testContext);
