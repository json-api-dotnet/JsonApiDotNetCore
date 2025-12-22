using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace DapperExample.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource]
public sealed class AccountRecovery : Identifiable<long>
{
    [Attr]
    public string? PhoneNumber { get; set; }

    [Attr]
    public string? EmailAddress { get; set; }

    [HasOne]
    public required LoginAccount Account { get; set; }
}
