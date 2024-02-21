using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.Headers;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.Headers")]
public sealed class Language : Identifiable<Guid>
{
    [Attr]
    public string Code { get; set; } = null!;

    [Attr]
    public string Name { get; set; } = null!;
}
