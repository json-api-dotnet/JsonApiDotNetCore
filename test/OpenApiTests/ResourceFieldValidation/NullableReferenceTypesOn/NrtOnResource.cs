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
    public required string NonNullableReferenceType { get; set; }

    [Attr]
    [Required]
    public required string RequiredNonNullableReferenceType { get; set; }

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
    public required NrtOnEmpty NonNullableToOne { get; set; }

    [Required]
    [HasOne]
    public required NrtOnEmpty RequiredNonNullableToOne { get; set; }

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
