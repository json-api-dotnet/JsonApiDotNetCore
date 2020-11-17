using System.Collections.Generic;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.IdObfuscation
{
    public sealed class BankAccount : ObfuscatedIdentifiable
    {
        [Attr]
        public string Iban { get; set; }

        [HasMany]
        public IList<DebitCard> Cards { get; set; }
    }
}
