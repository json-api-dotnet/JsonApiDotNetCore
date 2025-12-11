using JetBrains.Annotations;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.IdObfuscation;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(GenerateControllerEndpoints = JsonApiEndpoints.None)]
public sealed class DebitCard : ObfuscatedIdentifiable
{
    [Attr]
    public string OwnerName { get; set; } = null!;

    [Attr]
    public short PinCode { get; set; }

    [HasOne]
    public BankAccount Account { get; set; } = null!;
}
