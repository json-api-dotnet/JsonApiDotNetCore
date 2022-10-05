using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Experiments;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.Experiments")]
public sealed class Customer : Identifiable<long>
{
    [Attr]
    public string Name { get; set; } = null!;

    [HasOne]
    public Order? FirstOrder { get; set; }

    [HasOne]
    public Order? LastOrder { get; set; }

    [HasMany]
    public ISet<Order> Orders { get; set; } = new HashSet<Order>();
}
