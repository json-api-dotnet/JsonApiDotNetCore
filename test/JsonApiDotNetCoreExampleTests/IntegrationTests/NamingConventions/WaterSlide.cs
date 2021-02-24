using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.NamingConventions
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class WaterSlide : Identifiable
    {
        [Attr]
        public decimal LengthInMeters { get; set; }
    }
}
