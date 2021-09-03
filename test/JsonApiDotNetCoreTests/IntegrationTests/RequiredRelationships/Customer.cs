using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.RequiredRelationships
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Customer : Identifiable
    {
        [Attr]
        public string EmailAddress { get; set; }

        [HasMany]
        public ISet<Order> Orders { get; set; }
    }
}
