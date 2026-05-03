using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.Capabilities;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.Capabilities")]
public sealed class AllowCreateChangeCapability : Identifiable<long>
{
    [Attr]
    public string? AttributeOn { get; set; }

    [Attr(Capabilities = ~AttrCapabilities.AllowCreate)]
    public string? AttributeCreateOff { get; set; }

    [Attr(Capabilities = ~AttrCapabilities.AllowChange)]
    public string? AttributeChangeOff { get; set; }
}
