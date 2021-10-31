using System;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

#pragma warning disable AV1115 // Member or local function contains the word 'and', which suggests doing multiple things

namespace JsonApiDotNetCoreTests.IntegrationTests.RequiredRelationships
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Shipment : Identifiable<int>
    {
        [Attr]
        public string TrackAndTraceCode { get; set; } = null!;

        [Attr]
        public DateTimeOffset ShippedAt { get; set; }

        [HasOne]
        public Order Order { get; set; } = null!;
    }
}
