using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.CapabilitiesContrib;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.CapabilitiesContrib")]
public sealed class Comment : Identifiable<long>
{
    [Attr]
    public string Text { get; set; } = null!;
}
