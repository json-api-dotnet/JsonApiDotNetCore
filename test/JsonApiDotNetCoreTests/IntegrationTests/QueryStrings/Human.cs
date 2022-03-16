using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.QueryStrings")]
public abstract class Human : Identifiable<int>
{
    [Attr]
    public string Name { get; set; } = null!;

    [HasOne]
    public Man? Father { get; set; }

    [HasOne]
    public Woman? Mother { get; set; }

    [HasMany]
    public ISet<Human> Children { get; set; } = new HashSet<Human>();
}
