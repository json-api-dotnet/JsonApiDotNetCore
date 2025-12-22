using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace DatabasePerTenantExample.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource]
public sealed class Employee : Identifiable<Guid>
{
    [Attr]
    public required string FirstName { get; set; }

    [Attr]
    public required string LastName { get; set; }

    [Attr]
    public required string CompanyName { get; set; }
}
