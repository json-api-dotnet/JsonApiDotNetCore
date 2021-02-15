namespace JsonApiDotNetCore.Errors
{
    public sealed class MissingResourceInRelationship
    {
        public string RelationshipName { get; }
        public string ResourceType { get; }
        public string ResourceId { get; }

        public MissingResourceInRelationship(string relationshipName, string resourceType, string resourceId)
        {
            ArgumentGuard.NotNull(relationshipName, nameof(relationshipName));
            ArgumentGuard.NotNull(resourceType, nameof(resourceType));
            ArgumentGuard.NotNull(resourceId, nameof(resourceId));

            RelationshipName = relationshipName;
            ResourceType = resourceType;
            ResourceId = resourceId;
        }
    }
}
