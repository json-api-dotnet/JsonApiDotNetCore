using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace GettingStarted.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource]
public sealed class Person : Identifiable<long>
{
    [Attr]
    public required string Name { get; set; }

    [HasMany]
    public ICollection<Book> Books { get; set; } = new List<Book>();
}
