using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ClientSetNullOnDelete;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ClientSetNullOnDelete")]
public sealed class Order : Identifiable<long>
{
    [Attr]
    public decimal Amount { get; set; }

    [HasOne]
    public Customer Customer { get; set; } = null!;

    [HasOne]
    public Order? Parent { get; set; }
}
