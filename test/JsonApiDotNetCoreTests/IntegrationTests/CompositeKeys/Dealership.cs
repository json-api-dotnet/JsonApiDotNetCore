using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.CompositeKeys
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Dealership : Identifiable
    {
        [Attr]
        public string Address { get; set; }

        [HasMany]
        public ISet<Car> Inventory { get; set; }
    }
}
