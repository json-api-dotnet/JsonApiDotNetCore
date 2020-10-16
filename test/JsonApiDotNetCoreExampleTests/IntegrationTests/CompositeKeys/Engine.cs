using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.CompositeKeys
{
    public sealed class Engine : Identifiable
    {
        [Attr]
        public string SerialCode { get; set; }

        [HasOne] 
        public Car Car { get; set; }
    }
}
