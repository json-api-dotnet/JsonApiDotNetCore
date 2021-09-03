using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.EagerLoading
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class State : Identifiable
    {
        [Attr]
        public string Name { get; set; }

        [HasMany]
        public IList<City> Cities { get; set; }
    }
}
