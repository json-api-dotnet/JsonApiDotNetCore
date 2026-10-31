using JetBrains.Annotations;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Compound.TablePerConcreteType;

// The read tests cannot run against this mapping: seeding fails inside EF Core 10 (see CompoundTablePerConcreteTypeDbContext).
// When fixed, derive from CompoundInheritanceReadTests<CompoundTablePerConcreteTypeDbContext> as the TablePerHierarchy variant does.
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class CompoundTablePerConcreteTypeReadTests
{
    [Fact(Skip = "EF Core 10: ArgumentNullException (key) in ModificationCommand.HandleJson when inserting an entity whose abstract base type " +
        "declares JSON-mapped complex properties, with TPC mapping")]
    public void Read_tests_are_skipped()
    {
    }
}
