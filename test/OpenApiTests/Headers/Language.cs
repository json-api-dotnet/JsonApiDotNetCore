using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.Headers;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.Headers")]
public sealed class Language : Identifiable<Guid>
{
    [Attr]
    public required string Code { get; set; }

    [Attr]
    public required string Name { get; set; }
}
