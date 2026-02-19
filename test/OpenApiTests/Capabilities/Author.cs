using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.Capabilities;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.Capabilities")]
public sealed class Author : Identifiable<long>
{
    [Attr]
    public string Name { get; set; } = null!;
}
