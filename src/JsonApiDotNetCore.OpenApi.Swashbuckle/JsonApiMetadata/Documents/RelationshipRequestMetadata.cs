using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata.Documents;

internal sealed class RelationshipRequestMetadata : IJsonApiRequestMetadata
{
    public IReadOnlyDictionary<RelationshipAttribute, Type> DocumentTypesByRelationship { get; }

    public RelationshipRequestMetadata(IReadOnlyDictionary<RelationshipAttribute, Type> documentTypesByRelationship)
    {
        ArgumentNullException.ThrowIfNull(documentTypesByRelationship);

        DocumentTypesByRelationship = documentTypesByRelationship;
    }
}
