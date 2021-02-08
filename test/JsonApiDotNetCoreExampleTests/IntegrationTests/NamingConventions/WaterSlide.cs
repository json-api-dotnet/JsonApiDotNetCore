using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.NamingConventions
{
    public sealed class WaterSlide : Identifiable
    {
        [Attr]
        public decimal LengthInMeters { get; set; }
    }
}
