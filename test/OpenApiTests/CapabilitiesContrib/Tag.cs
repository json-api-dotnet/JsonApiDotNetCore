using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.CapabilitiesContrib;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.CapabilitiesContrib")]
public sealed class Tag : Identifiable<long>
{
    [Attr]
    public string Label { get; set; } = null!;
}
