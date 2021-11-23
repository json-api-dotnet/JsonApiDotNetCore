using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.RestrictedControllers
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Chair : Identifiable<int>
    {
        [Attr]
        public int LegCount { get; set; }

        [HasMany]
        public IList<Pillow> Pillows { get; set; } = new List<Pillow>();

        [HasOne]
        public Room? Room { get; set; }
    }
}
