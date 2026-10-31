using JetBrains.Annotations;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Compound.TablePerHierarchy;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class CompoundTablePerHierarchyWriteTests(
    IntegrationTestContext<TestableStartup<CompoundTablePerHierarchyDbContext>, CompoundTablePerHierarchyDbContext> testContext)
    : CompoundInheritanceWriteTests<CompoundTablePerHierarchyDbContext>(testContext);
