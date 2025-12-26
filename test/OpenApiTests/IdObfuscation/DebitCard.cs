using JetBrains.Annotations;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.IdObfuscation;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(GenerateControllerEndpoints = JsonApiEndpoints.None)]
public sealed class DebitCard : ObfuscatedIdentifiable
{
    [Attr]
    public required string OwnerName { get; set; }

    [Attr]
    public short PinCode { get; set; }

    [HasOne]
    public required BankAccount Account { get; set; }
}
