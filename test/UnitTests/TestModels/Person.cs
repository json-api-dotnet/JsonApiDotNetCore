using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace UnitTests.TestModels
{
    public class Person : Identifiable
    {
        [Attr] public string Name { get; set; }
        [HasMany] public ISet<Blog> Blogs { get; set; }
        [HasOne] public Food FavoriteFood { get; set; }
        [HasOne] public Song FavoriteSong { get; set; }
    }
}