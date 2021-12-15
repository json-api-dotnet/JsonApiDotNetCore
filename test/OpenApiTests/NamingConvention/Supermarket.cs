using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.NamingConvention;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests")]
public sealed class Supermarket : Identifiable<int>
{
    [Attr]
    public string NameOfCity { get; set; } = null!;

    [Attr]
    public SupermarketType Kind { get; set; }

    [HasOne]
    public StaffMember StoreManager { get; set; } = null!;

    [HasOne]
    public StaffMember? BackupStoreManager { get; set; }

    [HasMany]
    public ICollection<StaffMember> Cashiers { get; set; } = new HashSet<StaffMember>();
}
