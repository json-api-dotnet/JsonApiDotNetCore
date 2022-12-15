using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.QueryStrings")]
public sealed class BlogPost : Identifiable<int>
{
    [Attr]
    public string Caption { get; set; } = null!;

    [Attr]
    public string Url { get; set; } = null!;

    [HasOne]
    public WebAccount? Author { get; set; }

    [HasOne]
    public WebAccount? Reviewer { get; set; }

    [HasMany]
    public ISet<Human> Contributors { get; set; } = new HashSet<Human>();

    [HasMany]
    public ISet<Label> Labels { get; set; } = new HashSet<Label>();

    [HasMany]
    public ISet<Comment> Comments { get; set; } = new HashSet<Comment>();

#pragma warning disable CS0618 // Type or member is obsolete
    [HasOne(CanInclude = false)]
#pragma warning restore CS0618 // Type or member is obsolete
    public Blog? Parent { get; set; }
}
