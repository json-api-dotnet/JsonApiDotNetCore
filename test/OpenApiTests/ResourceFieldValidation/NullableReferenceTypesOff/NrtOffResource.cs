#nullable disable

using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.ResourceFieldValidation.NullableReferenceTypesOff;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(PublicName = "resources", ControllerNamespace = "OpenApiTests.ResourceFieldValidation")]
public sealed class NrtOffResource : Identifiable<long>
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
    public NrtOffEmpty ToOne { get; set; }

    [Required]
    [HasOne]
    public NrtOffEmpty RequiredToOne { get; set; }

    [HasMany]
    public ICollection<NrtOffEmpty> ToMany { get; set; } = new HashSet<NrtOffEmpty>();

    [Required]
    [HasMany]
    public ICollection<NrtOffEmpty> RequiredToMany { get; set; } = new HashSet<NrtOffEmpty>();
}
