using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.QueryStrings")]
public sealed class Label : Identifiable<long>
{
    [Attr]
    public required string Name { get; set; }

    [Attr]
    public LabelColor Color { get; set; }

    [HasMany]
    public ISet<BlogPost> Posts { get; set; } = new HashSet<BlogPost>();
}
