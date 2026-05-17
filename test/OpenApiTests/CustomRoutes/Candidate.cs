using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.CustomRoutes;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.CustomRoutes")]
public sealed class Candidate : Identifiable<Guid>
{
    [Attr]
    public string PersonName { get; set; } = null!;

    [Attr]
    public string? PartyName { get; set; }

    [HasMany]
    public ISet<Election> Elections { get; set; } = new HashSet<Election>();
}
