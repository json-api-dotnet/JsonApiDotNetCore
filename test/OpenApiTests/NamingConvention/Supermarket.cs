using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.NamingConventions
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
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
}
