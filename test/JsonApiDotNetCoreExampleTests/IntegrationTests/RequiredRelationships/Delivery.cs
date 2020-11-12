using System;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.RequiredRelationships
{
    public sealed class Delivery : Identifiable
    {
        [Attr]
        public string TrackAndTraceCode { get; set; }
        
        [Attr]
        public DateTime ShippedAt { get; set; } 

        [HasOne]
        public Order Order { get; set; }
    }
}
