using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class Label : Identifiable<int>
{
    [Attr]
    public string Name { get; set; } = null!;

    [Attr]
    public LabelColor Color { get; set; }

    [HasMany]
    public ISet<BlogPost> Posts { get; set; } = new HashSet<BlogPost>();
}
