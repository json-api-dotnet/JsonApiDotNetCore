using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Authorization.Scopes;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class MovieProduction
{
    [Attr]
    public decimal Budget { get; set; }

    [Attr]
    public string? StudioName { get; set; }

    [Attr]
    public List<string> Locations { get; set; } = [];
}
