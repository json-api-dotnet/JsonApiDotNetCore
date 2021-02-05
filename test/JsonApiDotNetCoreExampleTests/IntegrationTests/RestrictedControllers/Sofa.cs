using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.RestrictedControllers
{
    public sealed class Sofa : Identifiable
    {
        [Attr]
        public int SeatCount { get; set; }
    }
}
