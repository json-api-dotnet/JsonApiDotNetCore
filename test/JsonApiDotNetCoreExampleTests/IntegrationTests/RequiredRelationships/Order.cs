using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.RequiredRelationships
{
    public sealed class Order : Identifiable
    {
        [Attr]
        public decimal Amount { get; set; }

        [HasOne]
        public Customer Customer { get; set; }

        [HasOne]
        public Shipment Shipment { get; set; }
    }
}
