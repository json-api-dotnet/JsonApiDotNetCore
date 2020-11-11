using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ZeroKeys
{
    public sealed class Game : Identifiable<int?>
    {
        [Attr]
        public string Title { get; set; }

        [Attr]
        public Guid SessionToken { get; } = Guid.NewGuid();

        [HasMany]
        public ICollection<Player> ActivePlayers { get; set; }
    }
}
