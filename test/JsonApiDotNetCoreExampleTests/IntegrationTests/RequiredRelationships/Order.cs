using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.RequiredRelationships
{
    public sealed class Order : Identifiable
    {
        [Attr]
        public decimal Value { get; set; }

        [HasOne]
        public Customer Customer { get; set; }

        [HasOne]
        public Delivery Delivery { get; set; }
    }
}
