using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.SchemaProperties.NullableReferenceTypesEnabled.RelationshipsObject;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.SchemaProperties")]
public sealed class NrtEnabledModel : Identifiable<int>
{
    [HasOne]
    public RelationshipModel HasOne { get; set; } = null!;

    [Required]
    [HasOne]
    public RelationshipModel RequiredHasOne { get; set; } = null!;

    [HasOne]
    public RelationshipModel? NullableHasOne { get; set; }

    [Required]
    [HasOne]
    public RelationshipModel? NullableRequiredHasOne { get; set; }

    [HasMany]
    public ICollection<RelationshipModel> HasMany { get; set; } = new HashSet<RelationshipModel>();

    [Required]
    [HasMany]
    public ICollection<RelationshipModel> RequiredHasMany { get; set; } = new HashSet<RelationshipModel>();
}