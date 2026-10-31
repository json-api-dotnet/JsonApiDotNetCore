using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes.Constraints;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class BookDetails
{
    [Attr]
    public string Isbn { get; set; } = null!;

    [Attr]
    public int PageCount { get; set; }

    [Attr(IsCompound = true)]
    public BookEdition Edition { get; set; } = null!;
}
