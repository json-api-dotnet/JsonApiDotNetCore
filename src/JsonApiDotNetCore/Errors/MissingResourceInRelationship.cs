using System;

namespace JsonApiDotNetCore.Errors
{
    public sealed class MissingResourceInRelationship
    {
        public string RelationshipName { get; }
        public string ResourceType { get; }
        public string ResourceId { get; }

        public MissingResourceInRelationship(string relationshipName, string resourceType, string resourceId)
        {
            RelationshipName = relationshipName ?? throw new ArgumentNullException(nameof(relationshipName));
            ResourceType = resourceType ?? throw new ArgumentNullException(nameof(resourceType));
            ResourceId = resourceId ?? throw new ArgumentNullException(nameof(resourceId));
        }
    }
}
