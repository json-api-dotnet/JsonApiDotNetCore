using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.Capabilities;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.Capabilities")]
public sealed class Article : Identifiable<long>
{
    [Attr]
    public string Headline { get; set; } = null!;

    [HasOne(Capabilities = HasOneCapabilities.AllowSet)]
    public Writer? Writer { get; set; }

    [HasMany(Capabilities = HasManyCapabilities.AllowView)]
    public ISet<Category> Categories { get; set; } = new HashSet<Category>();

    [HasMany(Capabilities = HasManyCapabilities.AllowAdd)]
    public ISet<Tag> Tags { get; set; } = new HashSet<Tag>();

    [HasMany(Capabilities = HasManyCapabilities.AllowRemove)]
    public ISet<Comment> Comments { get; set; } = new HashSet<Comment>();
}
