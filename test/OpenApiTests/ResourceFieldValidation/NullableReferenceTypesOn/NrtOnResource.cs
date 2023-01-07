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
    public EmptyResource NonNullableToOne { get; set; } = null!;

    [Required]
    [HasOne]
    public EmptyResource RequiredNonNullableToOne { get; set; } = null!;

    [HasOne]
    public EmptyResource? NullableToOne { get; set; }

    [Required]
    [HasOne]
    public EmptyResource? RequiredNullableToOne { get; set; }

    [HasMany]
    public ICollection<EmptyResource> ToMany { get; set; } = new HashSet<EmptyResource>();

    [Required]
    [HasMany]
    public ICollection<EmptyResource> RequiredToMany { get; set; } = new HashSet<EmptyResource>();
}
