#nullable disable

using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.IdObfuscation
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class DebitCard : ObfuscatedIdentifiable
    {
        [Attr]
        public string OwnerName { get; set; }

        [Attr]
        public short PinCode { get; set; }

        [HasOne]
        public BankAccount Account { get; set; }
    }
}
