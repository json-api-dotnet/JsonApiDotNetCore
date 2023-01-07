using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.ResourceFieldValidation.NullableReferenceTypesOn;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(PublicName = "Resource", ControllerNamespace = "OpenApiTests.ResourceFieldValidation")]
public sealed class NrtOnResource : Identifiable<int>
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
    public Empty NonNullableToOne { get; set; } = null!;

    [Required]
    [HasOne]
    public Empty RequiredNonNullableToOne { get; set; } = null!;

    [HasOne]
    public Empty? NullableToOne { get; set; }

    [Required]
    [HasOne]
    public Empty? RequiredNullableToOne { get; set; }

    [HasMany]
    public ICollection<Empty> ToMany { get; set; } = new HashSet<Empty>();

    [Required]
    [HasMany]
    public ICollection<Empty> RequiredToMany { get; set; } = new HashSet<Empty>();
}
