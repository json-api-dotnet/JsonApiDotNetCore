using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ZeroKeys
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Game : Identifiable<int?>
    {
        [Attr]
        public string Title { get; set; }

        [NotMapped]
        [Attr]
        public Guid SessionToken => Guid.NewGuid();

        [HasMany]
        public ICollection<Player> ActivePlayers { get; set; }

        [HasOne]
        public Map ActiveMap { get; set; }

        [HasMany]
        public ICollection<Map> Maps { get; set; }
    }
}
