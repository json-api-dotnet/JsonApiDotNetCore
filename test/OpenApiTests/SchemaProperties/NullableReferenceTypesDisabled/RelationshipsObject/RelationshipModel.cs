#nullable disable
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.SchemaProperties.NullableReferenceTypesDisabled.RelationshipsObject;

[Resource]
public class RelationshipModel : Identifiable<int>
{
}
