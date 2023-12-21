using JetBrains.Annotations;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.TablePerConcreteType;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class TablePerConcreteTypeReadTests(
    IntegrationTestContext<TestableStartup<TablePerConcreteTypeDbContext>, TablePerConcreteTypeDbContext> testContext)
    : ResourceInheritanceReadTests<TablePerConcreteTypeDbContext>(testContext);
