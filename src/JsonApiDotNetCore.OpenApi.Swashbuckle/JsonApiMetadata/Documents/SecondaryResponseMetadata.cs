using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata.Documents;

internal sealed class SecondaryResponseMetadata(IReadOnlyDictionary<RelationshipAttribute, Type> documentTypesByRelationship)
    : NonPrimaryResponseMetadata(documentTypesByRelationship);
