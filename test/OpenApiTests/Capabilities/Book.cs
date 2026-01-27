using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.Capabilities;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.Capabilities")]
public sealed class Book : Identifiable<long>
{
    [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowChange)]
    public string Title { get; set; } = null!;

    [Attr(Capabilities = AttrCapabilities.AllowView)]
    public string Isbn { get; set; } = null!;

    [Attr(Capabilities = AttrCapabilities.AllowView)]
    public DateOnly PublishedOn { get; set; }

    [Attr(Capabilities = AttrCapabilities.AllowCreate)]
    public string DraftContent { get; set; } = null!;

    [Attr(Capabilities = AttrCapabilities.AllowChange)]
    public string InternalNotes { get; set; } = null!;

    [Attr(Capabilities = AttrCapabilities.None)]
    public string SecretCode { get; set; } = null!;

    [Attr]
    public bool HasEmptyTitle => string.IsNullOrEmpty(Title);

    [Attr]
    public bool IsDeleted { set => _ = value; }

    [HasOne(Capabilities = HasOneCapabilities.AllowView)]
    public Author? Author { get; set; }

    [HasMany(Capabilities = HasManyCapabilities.AllowSet)]
    public ISet<Review> Reviews { get; set; } = new HashSet<Review>();
}
