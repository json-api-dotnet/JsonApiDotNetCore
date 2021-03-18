using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ApiFormatMedataProvider
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Store : Identifiable
    {
        [Attr]
        public string Name { get; set; }

        [Attr]
        public string Address { get; set; }

        [HasMany]
        public ICollection<Product> Products { get; set; }
    }
}
