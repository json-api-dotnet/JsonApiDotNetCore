using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ZeroKeys
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    [Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ZeroKeys")]
    public sealed class Game : Identifiable<int?>
    {
        [Attr]
        public string Title { get; set; } = null!;

        [NotMapped]
        [Attr]
        public Guid SessionToken => Guid.NewGuid();

        [HasMany]
        public ICollection<Player> ActivePlayers { get; set; } = new List<Player>();

        [HasOne]
        public Map? ActiveMap { get; set; }

        [HasMany]
        public ICollection<Map> Maps { get; set; } = new List<Map>();
    }
}
