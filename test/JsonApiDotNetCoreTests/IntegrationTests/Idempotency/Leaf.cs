using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Idempotency;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.Idempotency")]
public sealed class Leaf : Identifiable<long>
{
    [Attr]
    public string Color { get; set; } = null!;

    [HasOne]
    public Branch Branch { get; set; } = null!;
}
