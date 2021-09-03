using System;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ZeroKeys
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Map : Identifiable<Guid?>
    {
        [Attr]
        public string Name { get; set; }

        [HasOne]
        public Game Game { get; set; }
    }
}
