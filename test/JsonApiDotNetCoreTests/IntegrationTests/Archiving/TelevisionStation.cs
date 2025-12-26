using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Archiving;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.Archiving")]
public sealed class TelevisionStation : Identifiable<long>
{
    [Attr]
    public required string Name { get; set; }

    [HasMany]
    public ISet<TelevisionBroadcast> Broadcasts { get; set; } = new HashSet<TelevisionBroadcast>();
}
