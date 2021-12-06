using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.NonJsonApiControllers;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.NonJsonApiControllers")]
public sealed class KnownResource : Identifiable<int>
{
    [Attr]
    public string? Value { get; set; }
}
