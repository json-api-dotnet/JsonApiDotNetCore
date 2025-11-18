using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.CapabilitiesContrib;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.CapabilitiesContrib")]
public sealed class Author : Identifiable<long>
{
    [Attr]
    public string Name { get; set; } = null!;
}
