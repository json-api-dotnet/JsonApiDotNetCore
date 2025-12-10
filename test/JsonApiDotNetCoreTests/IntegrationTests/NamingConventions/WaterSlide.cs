using JetBrains.Annotations;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.NamingConventions;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(GenerateControllerEndpoints = JsonApiEndpoints.None)]
public sealed class WaterSlide : Identifiable<long>
{
    [Attr]
    public decimal LengthInMeters { get; set; }
}
