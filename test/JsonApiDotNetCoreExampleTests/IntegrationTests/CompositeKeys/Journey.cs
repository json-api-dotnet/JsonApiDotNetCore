using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.CompositeKeys
{
    public sealed class Journey : Identifiable
    {
        [Attr]
        public string Destination { get; set; }

        [HasOne] 
        public Car Car { get; set; }
    }
}
