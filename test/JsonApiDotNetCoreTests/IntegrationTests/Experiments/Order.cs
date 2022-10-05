using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Experiments;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.Experiments")]
public sealed class Order : Identifiable<long>
{
    [Attr]
    public decimal Amount { get; set; }

    [HasOne]
    public Customer Customer { get; set; } = null!;

    [HasOne]
    public Order? Parent { get; set; }
}
