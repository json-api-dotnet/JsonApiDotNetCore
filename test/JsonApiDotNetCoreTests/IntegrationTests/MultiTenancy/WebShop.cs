using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.MultiTenancy
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class WebShop : Identifiable, IHasTenant
    {
        [Attr]
        public string Url { get; set; }

        public Guid TenantId { get; set; }

        [HasMany]
        public IList<WebProduct> Products { get; set; }
    }
}
