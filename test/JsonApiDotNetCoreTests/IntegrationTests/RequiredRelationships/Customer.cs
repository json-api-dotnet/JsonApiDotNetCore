using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.RequiredRelationships;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.RequiredRelationships")]
public sealed class Customer : Identifiable<long>
{
    [Attr]
    public required string EmailAddress { get; set; }

    [HasMany]
    public ISet<Order> Orders { get; set; } = new HashSet<Order>();
}
