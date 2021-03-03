using System;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.RequiredRelationships
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
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
