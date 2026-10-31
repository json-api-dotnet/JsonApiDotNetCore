using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes.Constraints;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes.Constraints")]
public sealed class Author : Identifiable<long>
{
    [Attr]
    public string Name { get; set; } = null!;

    [Attr(IsCompound = true)]
    public AuthorContact Contact { get; set; } = null!;

    [HasMany]
    public List<Book> Books { get; set; } = [];
}
