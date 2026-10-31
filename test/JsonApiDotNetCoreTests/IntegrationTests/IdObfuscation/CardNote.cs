using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.IdObfuscation;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class CardNote
{
    [Attr]
    public string Text { get; set; } = null!;

    [Attr]
    public int Priority { get; set; }
}