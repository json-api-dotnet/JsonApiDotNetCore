using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.Links;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class LinksInResourceIdentifierCollectionDocument
{
    [Required]
    public string Self { get; set; } = null!;

    public string Describedby { get; set; } = null!;

    [Required]
    public string Related { get; set; } = null!;

    [Required]
    public string First { get; set; } = null!;

    public string Last { get; set; } = null!;

    public string Prev { get; set; } = null!;

    public string Next { get; set; } = null!;
}
