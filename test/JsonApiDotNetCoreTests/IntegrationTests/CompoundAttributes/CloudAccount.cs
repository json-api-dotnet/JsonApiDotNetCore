using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes")]
public sealed class CloudAccount : Identifiable<long>
{
    [Attr(IsCompound = true)]
    public Contact EmergencyContact { get; set; } = null!;

    [Attr(IsCompound = true)]
    public Contact? BackupEmergencyContact { get; set; }

    [Attr(IsCompound = true)]
    public ISet<Contact> Contacts { get; set; } = new HashSet<Contact>();
}
