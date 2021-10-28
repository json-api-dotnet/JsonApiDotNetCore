using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.NamingConvention
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Supermarket : Identifiable
    {
        [Attr]
        public string NameOfCity { get; set; }

        [Attr]
        public SupermarketType Kind { get; set; }

        [HasOne]
        public StaffMember StoreManager { get; set; }

        [HasMany]
        public ICollection<StaffMember> Cashiers { get; set; }
    }
}
