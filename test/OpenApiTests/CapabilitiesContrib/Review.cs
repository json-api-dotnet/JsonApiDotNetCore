using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.CapabilitiesContrib;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.CapabilitiesContrib")]
public sealed class Review : Identifiable<long>
{
    [Attr]
    public string Content { get; set; } = null!;

    [Attr]
    public int Rating { get; set; }
}
