using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.ResourceFieldValidation.NullableReferenceTypesOn;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(PublicName = "resources", ControllerNamespace = "OpenApiTests.ResourceFieldValidation")]
public sealed class NrtOnResource : Identifiable<long>
{
    [Attr]
    public string NonNullableReferenceType { get; set; } = null!;

    [Attr]
    [Required]
    public string RequiredNonNullableReferenceType { get; set; } = null!;

    [Attr]
    public string? NullableReferenceType { get; set; }

    [Attr]
    [Required]
    public string? RequiredNullableReferenceType { get; set; }

    [Attr]
    public int ValueType { get; set; }

    [Attr]
    [Required]
    public int RequiredValueType { get; set; }

    [Attr]
    public int? NullableValueType { get; set; }

    [Attr]
    [Required]
    public int? RequiredNullableValueType { get; set; }

    [HasOne]
    public NrtOnEmpty NonNullableToOne { get; set; } = null!;

    [Required]
    [HasOne]
    public NrtOnEmpty RequiredNonNullableToOne { get; set; } = null!;

    [HasOne]
    public NrtOnEmpty? NullableToOne { get; set; }

    [Required]
    [HasOne]
    public NrtOnEmpty? RequiredNullableToOne { get; set; }

    [HasMany]
    public ICollection<NrtOnEmpty> ToMany { get; set; } = new HashSet<NrtOnEmpty>();

    [Required]
    [HasMany]
    public ICollection<NrtOnEmpty> RequiredToMany { get; set; } = new HashSet<NrtOnEmpty>();
}
