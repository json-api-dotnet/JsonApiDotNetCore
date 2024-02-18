using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.Headers;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.Headers")]
public sealed class Country : Identifiable<Guid>
{
    [Attr]
    public string Name { get; set; } = null!;

    [Attr]
    public int Population { get; set; }
}
