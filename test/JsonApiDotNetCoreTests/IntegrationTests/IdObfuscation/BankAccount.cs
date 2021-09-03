using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.IdObfuscation
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class BankAccount : ObfuscatedIdentifiable
    {
        [Attr]
        public string Iban { get; set; }

        [HasMany]
        public IList<DebitCard> Cards { get; set; }
    }
}
