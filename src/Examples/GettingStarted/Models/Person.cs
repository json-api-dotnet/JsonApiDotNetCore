using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace GettingStarted.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource]
public sealed class Person : Identifiable<int>
{
    [Attr]
    public string Name { get; set; } = null!;

    [Attr]
    public bool IsDeleted { get; set; }

    [HasMany]
    public ICollection<Book> Books { get; set; } = new List<Book>();

    [HasOne]
    public House? House { get; set; }
}

[Resource]
public abstract class House : Identifiable<int>;

[Resource]
public sealed class TinyHouse : House;

[Resource]
public sealed class BigHouse : House
{
    [Attr]
    public int? FloorCount { get; set; }
}
