using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata.Documents;

internal sealed class RelationshipResponseMetadata(IReadOnlyDictionary<RelationshipAttribute, Type> documentTypesByRelationship)
    : NonPrimaryResponseMetadata(documentTypesByRelationship);
