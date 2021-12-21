using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Idempotency;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.Idempotency")]
public sealed class Tree : Identifiable<long>
{
    [Attr]
    public decimal HeightInMeters { get; set; }

    [HasMany]
    public IList<Branch> Branches { get; set; } = new List<Branch>();
}
