using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.RequiredRelationships
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    [Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.RequiredRelationships")]
    public sealed class Customer : Identifiable<int>
    {
        [Attr]
        public string EmailAddress { get; set; } = null!;

        [HasMany]
        public ISet<Order> Orders { get; set; } = new HashSet<Order>();
    }
}
