using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.QueryStrings")]
public sealed class Man : Human
{
    [Attr]
    public bool HasBeard { get; set; }

    [Attr]
    public int Age { get; set; }

    [HasOne]
    public Woman? Wife { get; set; }

    [HasMany]
    public ISet<Human> Friends { get; set; } = new HashSet<Human>();

    [HasMany]
    public ISet<Man> SameGenderFriends { get; set; } = new HashSet<Man>();

    [HasMany]
    public ISet<Man> DrinkingBuddies { get; set; } = new HashSet<Man>();
}
