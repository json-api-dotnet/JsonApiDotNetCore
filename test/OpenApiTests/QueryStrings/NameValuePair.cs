using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.QueryStrings")]
public sealed class NameValuePair : Identifiable<long>
{
    [Attr]
    public required string Name { get; set; }

    [Attr]
    public string? Value { get; set; }

    [HasOne]
    public required Node Owner { get; set; }
}
