using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Idempotency;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.Idempotency")]
public sealed class Branch : Identifiable<long>
{
    [Attr]
    public decimal LengthInMeters { get; set; }

    [HasMany]
    public IList<Leaf> Leaves { get; set; } = new List<Leaf>();
}
