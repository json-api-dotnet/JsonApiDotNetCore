using JetBrains.Annotations;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.IdObfuscation;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(GenerateControllerEndpoints = JsonApiEndpoints.None)]
public sealed class BankAccount : ObfuscatedIdentifiable
{
    [Attr]
    public string Iban { get; set; } = null!;

    [HasMany]
    public IList<DebitCard> Cards { get; set; } = new List<DebitCard>();
}
