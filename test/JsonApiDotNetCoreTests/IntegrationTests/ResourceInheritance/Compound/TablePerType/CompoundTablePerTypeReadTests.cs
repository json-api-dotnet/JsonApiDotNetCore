using JetBrains.Annotations;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Compound.TablePerType;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class CompoundTablePerTypeReadTests
{
    [Fact(Skip = "EF Core 10: Sequence contains more than one element (fixed in EF Core 11)")]
    public void Read_tests_are_skipped()
    {
    }
}
