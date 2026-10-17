using JetBrains.Annotations;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Compound.TablePerType;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class CompoundTablePerTypeReadTests(
    IntegrationTestContext<TestableStartup<CompoundTablePerTypeDbContext>, CompoundTablePerTypeDbContext> testContext)
    : CompoundInheritanceReadTests<CompoundTablePerTypeDbContext>(testContext);
