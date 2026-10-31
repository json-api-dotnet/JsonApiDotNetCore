using JetBrains.Annotations;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Compound.TablePerConcreteType;

// When the EF Core 10 limitation described at CompoundTablePerConcreteTypeDbContext is fixed, derive from
// CompoundInheritanceWriteTests<CompoundTablePerConcreteTypeDbContext> as the TablePerHierarchy variant does.
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class CompoundTablePerConcreteTypeWriteTests
{
    [Fact(Skip = "EF Core 10: ArgumentNullException (key) in ModificationCommand.HandleJson when inserting an entity whose abstract base type " +
        "declares JSON-mapped complex properties, with TPC mapping")]
    public void Write_tests_are_skipped()
    {
    }
}
