using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.QueryStrings")]
public sealed class NameValuePair : Identifiable<long>
{
    [Attr]
    public string Name { get; set; } = null!;

    [Attr]
    public string? Value { get; set; }

    [HasOne]
    public Node Owner { get; set; } = null!;
}
