using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class Holiday
{
    [Attr]
    public string Name { get; set; } = null!;

    [Attr]
    public DateOnly Date { get; set; }

    [Attr]
    public bool IsPublic { get; set; }
}
