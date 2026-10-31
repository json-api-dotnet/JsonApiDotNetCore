using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes.Constraints;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes.Constraints")]
public sealed class Book : Identifiable<long>
{
    [Attr]
    public string Title { get; set; } = null!;

    [Attr]
    public bool IsHidden { get; set; }

    [Attr(IsCompound = true)]
    public BookDetails Details { get; set; } = null!;

    [HasOne]
    public Author? Author { get; set; }
}
