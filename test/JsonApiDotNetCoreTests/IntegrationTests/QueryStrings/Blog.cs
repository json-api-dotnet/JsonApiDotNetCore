using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.QueryStrings")]
public sealed class Blog : Identifiable<long>
{
    [Attr]
    public required string Title { get; set; }

    [Attr]
    public required string PlatformName { get; set; }

    [Attr(Capabilities = AttrCapabilities.All & ~(AttrCapabilities.AllowCreate | AttrCapabilities.AllowChange))]
    public bool ShowAdvertisements => PlatformName.EndsWith("(using free account)", StringComparison.Ordinal);

    public bool IsPublished { get; set; }

    [HasMany]
    public IList<BlogPost> Posts { get; set; } = new List<BlogPost>();

    [HasOne]
    public WebAccount? Owner { get; set; }
}
