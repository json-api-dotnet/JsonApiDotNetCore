using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata.Documents;

internal sealed class EmptyRelationshipResponseMetadata : IJsonApiResponseMetadata
{
    public IReadOnlyCollection<RelationshipAttribute> Relationships { get; }

    public EmptyRelationshipResponseMetadata(IReadOnlyCollection<RelationshipAttribute> relationships)
    {
        ArgumentNullException.ThrowIfNull(relationships);

        Relationships = relationships;
    }
}
