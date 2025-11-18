using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.AttributeTypes;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.AttributeTypes")]
public sealed class Book : Identifiable<int>
{
    // Visible in GET, PATCH, included in all operations
    [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowChange)]
    public string Title { get; set; } = null!;

    // Only visible in GET
    [Attr(Capabilities = AttrCapabilities.AllowView)]
    public string Isbn { get; set; } = null!;

    // Only visible in GET
    [Attr(Capabilities = AttrCapabilities.AllowView)]
    public DateOnly PublishedOn { get; set; }

    // Only usable in POST
    [Attr(Capabilities = AttrCapabilities.AllowCreate)]
    public string DraftContent { get; set; } = null!;

    // Only usable in PATCH
    [Attr(Capabilities = AttrCapabilities.AllowChange)]
    public string InternalNotes { get; set; } = null!;

    // No visibility or modifiers whatsoever
    [Attr(Capabilities = AttrCapabilities.None)]
    public string SecretCode { get; set; } = null!;

    [HasOne(Capabilities = HasOneCapabilities.AllowView)]
    public Author? Author { get; set; }

    [HasMany(Capabilities = HasManyCapabilities.AllowSet)]
    public ISet<Review> Reviews { get; set; } = new HashSet<Review>();
}
