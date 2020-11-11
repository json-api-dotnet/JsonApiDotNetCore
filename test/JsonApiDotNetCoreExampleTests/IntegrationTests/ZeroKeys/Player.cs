using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ZeroKeys
{
    public sealed class Player : Identifiable<string>
    {
        [Attr]
        public string EmailAddress { get; set; }

        [HasOne]
        public Game ActiveGame { get; set; }

        [HasMany]
        public ICollection<Game> RecentlyPlayed { get; set; }
    }
}
