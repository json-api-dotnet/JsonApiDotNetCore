using System;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.RequiredRelationships
{
    public sealed class Shipment : Identifiable
    {
        [Attr]
        public string TrackAndTraceCode { get; set; }
        
        [Attr]
        public DateTimeOffset ShippedAt { get; set; } 

        [HasOne]
        public Order Order { get; set; }
    }
}
