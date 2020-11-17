using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.RequiredRelationships
{
    public sealed class Customer : Identifiable
    {
        [Attr]
        public string EmailAddress { get; set; }

        [HasMany]
        public ISet<Order> Orders { get; set; }
    }
}
