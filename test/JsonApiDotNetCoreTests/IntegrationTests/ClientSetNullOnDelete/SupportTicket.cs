using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ClientSetNullOnDelete;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ClientSetNullOnDelete")]
public sealed class SupportTicket : Identifiable<long>
{
    [Attr]
    public string Description { get; set; } = null!;

    [HasOne]
    public Order? Order { get; set; }
}
