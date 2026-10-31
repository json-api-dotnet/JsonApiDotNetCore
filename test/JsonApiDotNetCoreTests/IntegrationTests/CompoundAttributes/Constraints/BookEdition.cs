using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes.Constraints;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class BookEdition
{
    [Attr]
    public int Number { get; set; }

    [Attr]
    public int Year { get; set; }
}
