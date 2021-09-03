using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Archiving
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class TelevisionStation : Identifiable
    {
        [Attr]
        public string Name { get; set; }

        [HasMany]
        public ISet<TelevisionBroadcast> Broadcasts { get; set; }
    }
}
