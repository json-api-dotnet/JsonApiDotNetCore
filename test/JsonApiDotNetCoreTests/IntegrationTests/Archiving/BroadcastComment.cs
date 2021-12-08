using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Archiving;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.Archiving")]
public sealed class BroadcastComment : Identifiable<int>
{
    [Attr]
    public string Text { get; set; } = null!;

    [Attr]
    public DateTimeOffset CreatedAt { get; set; }

    [HasOne]
    public TelevisionBroadcast AppliesTo { get; set; } = null!;
}
