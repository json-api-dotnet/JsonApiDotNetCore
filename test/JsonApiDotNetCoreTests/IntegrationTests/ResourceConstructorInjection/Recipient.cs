using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceConstructorInjection;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class Recipient
{
    [Attr]
    public string Name { get; set; } = null!;

    [Attr]
    public string? EmailAddress { get; set; }
}
