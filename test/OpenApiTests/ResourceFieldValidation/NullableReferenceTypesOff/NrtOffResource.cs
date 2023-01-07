#nullable disable

using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.ResourceFieldValidation.NullableReferenceTypesOff;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(PublicName = "Resource", ControllerNamespace = "OpenApiTests.ResourceFieldValidation")]
public sealed class NrtOffResource : Identifiable<int>
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
    public Empty ToOne { get; set; }

    [Required]
    [HasOne]
    public Empty RequiredToOne { get; set; }

    [HasMany]
    public ICollection<Empty> ToMany { get; set; } = new HashSet<Empty>();

    [Required]
    [HasMany]
    public ICollection<Empty> RequiredToMany { get; set; } = new HashSet<Empty>();
}
