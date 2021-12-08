using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.IdObfuscation;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class BankAccount : ObfuscatedIdentifiable
{
    [Attr]
    public string Iban { get; set; } = null!;

    [HasMany]
    public IList<DebitCard> Cards { get; set; } = new List<DebitCard>();
}
