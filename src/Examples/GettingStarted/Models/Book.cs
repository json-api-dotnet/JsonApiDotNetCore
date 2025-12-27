using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace GettingStarted.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource]
public sealed class Book : Identifiable<long>
{
    [Attr]
    public required string Title { get; set; }

    [Attr]
    public int PublishYear { get; set; }

    [HasOne]
    public required Person Author { get; set; }
}
