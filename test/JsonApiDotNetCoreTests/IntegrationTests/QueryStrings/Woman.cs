using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.QueryStrings")]
public sealed class Woman : Human
{
    [Attr]
    public string MaidenName { get; set; } = null!;

    [HasOne]
    public Man? Husband { get; set; }

    [HasMany]
    public ISet<Human> Friends { get; set; } = new HashSet<Human>();

    [HasMany]
    public ISet<Woman> SameGenderFriends { get; set; } = new HashSet<Woman>();
}
