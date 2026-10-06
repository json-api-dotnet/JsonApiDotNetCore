using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ClientSetNullOnDelete;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[NoResource]
public sealed class OrderAudit
{
    public long Id { get; set; }

    public string Note { get; set; } = null!;

    public Order? Order { get; set; }
}
