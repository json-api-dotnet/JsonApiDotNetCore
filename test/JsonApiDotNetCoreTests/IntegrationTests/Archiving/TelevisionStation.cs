#nullable disable

using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Archiving
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class TelevisionStation : Identifiable<int>
    {
        [Attr]
        public string Name { get; set; }

        [HasMany]
        public ISet<TelevisionBroadcast> Broadcasts { get; set; }
    }
}
