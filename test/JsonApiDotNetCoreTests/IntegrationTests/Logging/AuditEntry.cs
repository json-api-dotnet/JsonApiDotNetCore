using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Logging;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.Logging")]
public sealed class AuditEntry : Identifiable<long>
{
    [Attr]
    public required string UserName { get; set; }

    [Attr]
    public DateTimeOffset CreatedAt { get; set; }
}
