using JetBrains.Annotations;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.TablePerType;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class TablePerTypeReadTests(IntegrationTestContext<TestableStartup<TablePerTypeDbContext>, TablePerTypeDbContext> testContext)
    : ResourceInheritanceReadTests<TablePerTypeDbContext>(testContext);
