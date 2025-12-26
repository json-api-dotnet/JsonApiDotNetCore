using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.UnitTests.Serialization.Response.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class Person : Identifiable<long>
{
    [Attr]
    public required string Name { get; set; }

    [HasMany]
    public ISet<Blog> Blogs { get; set; } = new HashSet<Blog>();

    [HasOne]
    public required Food FavoriteFood { get; set; }

    [HasOne]
    public required Song FavoriteSong { get; set; }
}
