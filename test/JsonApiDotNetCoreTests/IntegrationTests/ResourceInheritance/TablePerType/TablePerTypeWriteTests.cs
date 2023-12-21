using JetBrains.Annotations;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.TablePerType;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class TablePerTypeWriteTests(IntegrationTestContext<TestableStartup<TablePerTypeDbContext>, TablePerTypeDbContext> testContext)
    : ResourceInheritanceWriteTests<TablePerTypeDbContext>(testContext);
