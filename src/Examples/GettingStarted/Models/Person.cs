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

    [Attr(IsCompound = true)]
    public required Address LivingAddress { get; set; }

    [Attr(IsCompound = true)]
    public Address? MailAddress { get; set; }

    // OwnsMany with nullable element type is unsupported by EF Core.
    [Attr(IsCompound = true)]
    public List<Address>? Addresses { get; set; }

    [Attr]
    public List<string?> NamesOfChildren { get; set; } = [];

    [Attr]
    public List<int?> AgesOfChildren { get; set; } = [];

    [HasMany]
    public ICollection<Book> Books { get; set; } = new List<Book>();
}
