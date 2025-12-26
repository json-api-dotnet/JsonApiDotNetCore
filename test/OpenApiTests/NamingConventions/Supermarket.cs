using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.NamingConventions;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.NamingConventions")]
public sealed class Supermarket : Identifiable<long>
{
    [Attr]
    public required string NameOfCity { get; set; }

    [Attr]
    public SupermarketType Kind { get; set; }

    [HasOne]
    public required StaffMember StoreManager { get; set; }

    [HasOne]
    public StaffMember? BackupStoreManager { get; set; }

    [HasMany]
    public ICollection<StaffMember> Cashiers { get; set; } = new HashSet<StaffMember>();
}
