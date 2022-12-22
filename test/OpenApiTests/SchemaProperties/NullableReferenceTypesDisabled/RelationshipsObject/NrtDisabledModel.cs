using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

#nullable disable 

namespace OpenApiTests.SchemaProperties.NullableReferenceTypesDisabled.RelationshipsObject;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.SchemaProperties")]
public sealed class NrtDisabledModel : Identifiable<int>
{
    [HasOne] public RelationshipModel HasOne { get; set; }
    [Required] [HasOne] public RelationshipModel  RequiredHasOne { get; set; }
    [HasMany] public ICollection<RelationshipModel>  HasMany { get; set; } = new HashSet<RelationshipModel>();
    [Required] [HasMany] public ICollection<RelationshipModel>  RequiredHasMany { get; set; } = new HashSet<RelationshipModel>();
}