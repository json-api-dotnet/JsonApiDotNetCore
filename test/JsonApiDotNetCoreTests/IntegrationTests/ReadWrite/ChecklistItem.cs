using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ReadWrite;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ChecklistItem
{
    [Attr]
    public string? Text { get; set; }

    [Attr]
    public bool IsDone { get; set; }
}
