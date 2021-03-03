using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace UnitTests.TestModels
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Person : Identifiable
    {
        [Attr]
        public string Name { get; set; }

        [HasMany]
        public ISet<Blog> Blogs { get; set; }

        [HasOne]
        public Food FavoriteFood { get; set; }

        [HasOne]
        public Song FavoriteSong { get; set; }
    }
}
