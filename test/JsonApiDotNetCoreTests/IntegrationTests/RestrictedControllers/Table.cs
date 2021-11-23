using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.RestrictedControllers
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Table : Identifiable<int>
    {
        [Attr]
        public int LegCount { get; set; }

        [HasMany]
        public IList<Chair> Chairs { get; set; } = new List<Chair>();

        [HasOne]
        public Room? Room { get; set; }
    }
}
