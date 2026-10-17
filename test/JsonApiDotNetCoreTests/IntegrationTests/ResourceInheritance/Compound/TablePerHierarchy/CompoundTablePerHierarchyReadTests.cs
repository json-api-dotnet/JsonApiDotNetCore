using JetBrains.Annotations;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Compound.TablePerHierarchy;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class CompoundTablePerHierarchyReadTests(
    IntegrationTestContext<TestableStartup<CompoundTablePerHierarchyDbContext>, CompoundTablePerHierarchyDbContext> testContext)
    : CompoundInheritanceReadTests<CompoundTablePerHierarchyDbContext>(testContext);
