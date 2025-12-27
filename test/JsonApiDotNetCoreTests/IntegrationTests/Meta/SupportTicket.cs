using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Meta;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.Meta")]
public sealed class SupportTicket : Identifiable<long>
{
    [Attr]
    public required string Description { get; set; }

    [HasOne]
    public ProductFamily? ProductFamily { get; set; }
}
