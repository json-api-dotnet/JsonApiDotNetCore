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
        ArgumentGuard.NotNullNorEmpty(relationshipName);
        ArgumentGuard.NotNullNorEmpty(resourceType);
        ArgumentGuard.NotNullNorEmpty(resourceId);

        RelationshipName = relationshipName;
        ResourceType = resourceType;
        ResourceId = resourceId;
    }
}
