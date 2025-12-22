using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace DapperExample.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource]
public sealed class LoginAccount : Identifiable<long>
{
    [Attr]
    public required string UserName { get; set; }

    public DateTimeOffset? LastUsedAt { get; set; }

    [HasOne]
    public required AccountRecovery Recovery { get; set; }

    [HasOne]
    public required Person Person { get; set; }
}
