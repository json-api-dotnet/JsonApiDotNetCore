#nullable disable

using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.ResourceFieldValidation.NullableReferenceTypesDisabled;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(PublicName = "Resource", ControllerNamespace = "OpenApiTests.ResourceFieldValidation")]
public sealed class NrtDisabledResource : Identifiable<int>
{
    [Attr]
    public string ReferenceType { get; set; }

    [Attr]
    [Required]
    public string RequiredReferenceType { get; set; }

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
    public EmptyResource ToOne { get; set; }

    [Required]
    [HasOne]
    public EmptyResource RequiredToOne { get; set; }

    [HasMany]
    public ICollection<EmptyResource> ToMany { get; set; } = new HashSet<EmptyResource>();

    [Required]
    [HasMany]
    public ICollection<EmptyResource> RequiredToMany { get; set; } = new HashSet<EmptyResource>();
}
