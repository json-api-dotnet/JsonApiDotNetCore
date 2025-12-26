using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompositeKeys;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.CompositeKeys")]
public sealed class Engine : Identifiable<long>
{
    [Attr]
    public required string SerialCode { get; set; }

    [HasOne]
    public Car? Car { get; set; }
}
