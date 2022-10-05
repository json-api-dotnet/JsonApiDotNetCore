using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Experiments;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.Experiments")]
public sealed class ShoppingBasket : Identifiable<long>
{
    [Attr]
    public int ProductCount { get; set; }

    [HasOne]
    public Order? CurrentOrder { get; set; }
}
