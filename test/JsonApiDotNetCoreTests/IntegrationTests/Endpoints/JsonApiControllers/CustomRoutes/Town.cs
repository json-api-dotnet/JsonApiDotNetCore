using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Endpoints.JsonApiControllers.CustomRoutes;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.Endpoints.JsonApiControllers.CustomRoutes")]
public sealed class Town : Identifiable<long>
{
    [Attr]
    public string Name { get; set; } = null!;

    [Attr]
    public double Latitude { get; set; }

    [Attr]
    public double Longitude { get; set; }

    [Attr]
    public string? FounderName { get; set; }

    [HasMany]
    public ISet<Civilian> Civilians { get; set; } = new HashSet<Civilian>();
}
