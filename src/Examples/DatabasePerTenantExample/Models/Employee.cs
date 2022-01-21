using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace DatabasePerTenantExample.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource]
public sealed class Employee : Identifiable<Guid>
{
    [Attr]
    public string FirstName { get; set; } = null!;

    [Attr]
    public string LastName { get; set; } = null!;

    [Attr]
    public string CompanyName { get; set; } = null!;
}
