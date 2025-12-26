using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Meta;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.Meta")]
public sealed class ProductFamily : Identifiable<long>
{
    [Attr]
    public required string Name { get; set; }

    [HasMany]
    public IList<SupportTicket> Tickets { get; set; } = new List<SupportTicket>();
}
