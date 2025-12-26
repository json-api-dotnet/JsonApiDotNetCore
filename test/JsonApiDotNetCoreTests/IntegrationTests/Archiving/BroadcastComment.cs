using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Archiving;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.Archiving")]
public sealed class BroadcastComment : Identifiable<long>
{
    [Attr]
    public required string Text { get; set; }

    [Attr]
    public DateTimeOffset CreatedAt { get; set; }

    [HasOne]
    public required TelevisionBroadcast AppliesTo { get; set; }
}
