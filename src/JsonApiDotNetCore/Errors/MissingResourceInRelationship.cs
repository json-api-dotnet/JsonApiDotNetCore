using JetBrains.Annotations;

namespace JsonApiDotNetCore.Errors;

[PublicAPI]
public sealed class MissingResourceInRelationship
{
    public string RelationshipName { get; }
    public string ResourceType { get; }
    public string ResourceId { get; }

    public MissingResourceInRelationship(string relationshipName, string resourceType, string resourceId)
    {
        ArgumentGuard.NotNull(relationshipName);
        ArgumentGuard.NotNull(resourceType);
        ArgumentGuard.NotNull(resourceId);

        RelationshipName = relationshipName;
        ResourceType = resourceType;
        ResourceId = resourceId;
    }
}
