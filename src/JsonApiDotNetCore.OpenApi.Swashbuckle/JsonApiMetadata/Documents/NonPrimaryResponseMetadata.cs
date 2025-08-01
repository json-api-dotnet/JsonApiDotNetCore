using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata.Documents;

internal class NonPrimaryResponseMetadata : IJsonApiResponseMetadata
{
    public IReadOnlyDictionary<RelationshipAttribute, Type> DocumentTypesByRelationship { get; }

    protected NonPrimaryResponseMetadata(IReadOnlyDictionary<RelationshipAttribute, Type> documentTypesByRelationship)
    {
        ArgumentNullException.ThrowIfNull(documentTypesByRelationship);

        DocumentTypesByRelationship = documentTypesByRelationship;
    }
}
