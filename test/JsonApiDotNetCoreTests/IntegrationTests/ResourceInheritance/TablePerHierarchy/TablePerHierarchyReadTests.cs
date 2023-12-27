using JetBrains.Annotations;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.TablePerHierarchy;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class TablePerHierarchyReadTests(IntegrationTestContext<TestableStartup<TablePerHierarchyDbContext>, TablePerHierarchyDbContext> testContext)
    : ResourceInheritanceReadTests<TablePerHierarchyDbContext>(testContext);
