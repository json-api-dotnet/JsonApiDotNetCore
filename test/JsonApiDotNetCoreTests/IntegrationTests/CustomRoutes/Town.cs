using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.CustomRoutes;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.CustomRoutes")]
public sealed class Town : Identifiable<int>
{
    [Attr]
    public string Name { get; set; } = null!;

    [Attr]
    public double Latitude { get; set; }

    [Attr]
    public double Longitude { get; set; }

    [HasMany]
    public ISet<Civilian> Civilians { get; set; } = new HashSet<Civilian>();
}
