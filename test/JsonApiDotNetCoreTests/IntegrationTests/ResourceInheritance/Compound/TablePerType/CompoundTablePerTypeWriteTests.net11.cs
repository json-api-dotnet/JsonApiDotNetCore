using JetBrains.Annotations;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Compound.TablePerType;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class CompoundTablePerTypeWriteTests(
    IntegrationTestContext<TestableStartup<CompoundTablePerTypeDbContext>, CompoundTablePerTypeDbContext> testContext)
    : CompoundInheritanceWriteTests<CompoundTablePerTypeDbContext>(testContext);
