using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ClientSetNullOnDelete;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ClientSetNullOnDelete")]
public sealed class ShoppingBasket : Identifiable<long>
{
    [Attr]
    public int ProductCount { get; set; }

    [HasOne]
    public Order? CurrentOrder { get; set; }
}
