using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Logging;

/// <summary>
/// A compound type that is mapped in EF Core as an owned entity type, which makes the resource graph builder encounter it as a non-identifiable type.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class AuditOrigin
{
    [Attr]
    public string? IpAddress { get; set; }

    [Attr]
    public string? Application { get; set; }
}
