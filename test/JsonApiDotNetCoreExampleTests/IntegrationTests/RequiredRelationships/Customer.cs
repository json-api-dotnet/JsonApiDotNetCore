using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.RequiredRelationships
{
    public sealed class Customer : Identifiable
    {
        [Attr]
        public string Address { get; set; }

        [HasMany]
        public ISet<Order> Orders { get; set; }
    }
}
