using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.UnitTests.Serialization.Response.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Person : Identifiable<int>
    {
        [Attr]
        public string Name { get; set; } = null!;

        [HasMany]
        public ISet<Blog> Blogs { get; set; } = new HashSet<Blog>();

        [HasOne]
        public Food FavoriteFood { get; set; } = null!;

        [HasOne]
        public Song FavoriteSong { get; set; } = null!;
    }
}
