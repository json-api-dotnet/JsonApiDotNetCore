using System;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ZeroKeys
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Map : Identifiable<Guid?>
    {
        [Attr]
        public string Name { get; set; } = null!;

        [HasOne]
        public Game? Game { get; set; }
    }
}
